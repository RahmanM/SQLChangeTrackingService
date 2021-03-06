﻿using Serilog;
using Sql.ChangeTracking.Common;
using Sql.ChangeTracking.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace Sql.ChangeTracking.Wcf
{
    public class SqlChangeTrackingWcfService : IChangeTrackingSubscriptions
    {
        private object locker = new object();
        private Dictionary<Subscriber, IEventNotificationCallback> Subscribers = new Dictionary<Subscriber, IEventNotificationCallback>();

        public ILogger Logger { get; set; }

        public void Subscribe(string id, string tableName)
        {
            try
            {
                IEventNotificationCallback callback = OperationContext.Current.GetCallbackChannel<IEventNotificationCallback>();
                lock (locker)
                {
                    Subscriber subscriber = new Subscriber();
                    subscriber.Id = id;
                    subscriber.TableInterested = tableName;
                    Subscribers.Add(subscriber, callback);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                throw;
            }
        }

        public void TableChanged(UspTableVersionChangeTrackingReturnModel table)
        {
            // get all the subscribers
            try
            {
                var subscriberKeys = (from c in Subscribers
                                      select c.Key).ToList();

                for (int i = subscriberKeys.Count-1; i >= 0 ; i--)
                {
                    var KeyValue = subscriberKeys.ElementAt(i);

                    IEventNotificationCallback callback = Subscribers[KeyValue];
                    if (((ICommunicationObject)callback).State == CommunicationState.Opened)
                    {
                        //call back only those subscribers who are interested in this fileType
                        if (string.Equals(KeyValue.TableInterested, table.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            callback.PublishTableChange(table.Name);
                        }
                    }
                    else
                    {
                        //These subscribers are no longer active. Delete them from subscriber list
                        subscriberKeys.Remove(KeyValue);
                        Subscribers.Remove(KeyValue);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                throw;
            }

        }

        public void Unsubscribe(string id, string tableName)
        {
            try
            {
                lock (locker)
                {
                    var SubToBeDeleted = from c in Subscribers.Keys
                                         where c.Id == id && c.TableInterested == tableName
                                         select c;

                    if (SubToBeDeleted.Any())
                    {
                        Subscribers.Remove(SubToBeDeleted.First());
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                throw;
            }
        }
    }
}
