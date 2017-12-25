﻿using Sql.ChangeTracking.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Sql.ChangeTracking.Wcf
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "SqlChangeTrackingWcfService" in both code and config file together.
    public class SqlChangeTrackingWcfService : IChangeTrackingSubscriptions
    {
        private object locker = new object();
        private Dictionary<Subscriber, IEventNotificationCallback> Subscribers = new Dictionary<Subscriber, IEventNotificationCallback>();

        public void Subscribe(string id, string tableName)
        {
            try
            {
                IEventNotificationCallback callback = OperationContext.Current.GetCallbackChannel<IEventNotificationCallback>();
                lock (locker)
                {
                    Debug.WriteLine("Adding client");

                    Subscriber subscriber = new Subscriber();
                    subscriber.Id = id;
                    subscriber.TableInterested = tableName;
                    Subscribers.Add(subscriber, callback);
                }
            }
            catch
            {
                // TODO Rahman: fill the stub
            }
        }

        public void TableChanged(string tableName)
        {
            //get all the subscribers
            var subscriberKeys = (from c in Subscribers
                                  select c.Key).ToList();

            Debug.WriteLine("TableChanged" + subscriberKeys.Count);

            foreach (var item in subscriberKeys)
            {
                Debug.WriteLine("TableChanged" + item.Id);

                IEventNotificationCallback callback = Subscribers[item];
                if (((ICommunicationObject)callback).State == CommunicationState.Opened)
                {
                    //call back only those subscribers who are interested in this fileType
                    if (string.Equals(item.TableInterested, tableName, StringComparison.OrdinalIgnoreCase))
                    {
                        callback.PublishTableChange(tableName);
                    }
                }
                else
                {
                    //These subscribers are no longer active. Delete them from subscriber list
                    subscriberKeys.Remove(item);
                    Subscribers.Remove(item);
                }
            }

        }

        public void Unsubscribe(string id, string tableName)
        {
            try
            {
                lock (locker)
                {
                    var SubToBeDeleted = from c in Subscribers.Keys
                                         where c.Id == id
                                         select c;

                    if (SubToBeDeleted.Any())
                    {
                        Subscribers.Remove(SubToBeDeleted.First());
                    }
                }
            }
            catch
            {
                // TODO Rahman: fill the stub
            }
        }
    }
}
