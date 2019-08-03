using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using HP.HPTRIM.SDK;
using Newtonsoft.Json;
using System;

namespace com.cmramble.ml.casefiles
{
    internal class ModelOperator
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Database db = null;
        private readonly PublisherServiceApiClient _pub;
        private readonly TopicName _topic;
        private static readonly string ProjectId = "gcp-demo-cm";
        private static readonly string TopicName = "cm-casefile-mlmodel-process";
        public ModelOperator(Database db)
        {
            this.db = db;
            this._pub = PublisherServiceApiClient.Create();
            this._topic = new TopicName(ProjectId, TopicName);
        }

        internal void ProcessEvent(Database db, TrimEvent evt)
        {
            // if this is a record that was just added
            if (evt.EventType == Events.ObjectAdded && evt.ObjectType == BaseObjectTypes.Record)
            {
                // check that this record was added into a folder that has an ML operator attached
                Record record = new Record(db, evt.ObjectUri);
                if (record != null)
                {
                    Record container = record.Container;
                    // check that there is a container
                    if (container != null)
                    {
                        Classification classification = container.Classification;
                        // check that there is a classification attached
                        if (classification != null)
                        {
                            FieldDefinition modelOperatorField = new FieldDefinition(db, "mlmodel");
                            // check that there is a model operator definition
                            if (modelOperatorField != null)
                            {
                                string modelId = classification.GetFieldValueAsString(modelOperatorField, StringDisplayType.Default, false);
                                // check that there is some value for this classification's mlmodel property
                                if (!string.IsNullOrWhiteSpace(modelId))
                                {
                                    // okay we have something we can work with.... let's ship it off to the pubsub topic
                                    EnqueueRecord(record, modelId);
                                }
                            } else { log.Debug($"Event skipped because container classification has no ML Model defined.  Type:{evt.EventType}, ObjectType:{evt.ObjectType}, ObjectUri:{evt.ObjectUri}, RecordNumber:{record.Number}, ContainerNumber:{container.Number}, Classification:{classification.Name}"); }
                        } else { log.Debug($"Event skipped because container has no classification.  Type:{evt.EventType}, ObjectType:{evt.ObjectType}, ObjectUri:{evt.ObjectUri}, RecordNumber:{record.Number}, ContainerNumber:{container.Number}"); }
                    } else { log.Debug($"Event skipped because record has no container.  Type:{evt.EventType}, ObjectType:{evt.ObjectType}, ObjectUri:{evt.ObjectUri}, RecordNumber:{record.Number}"); }
                } else { log.Error($"Event object not found.  Type:{evt.EventType}, ObjectType:{evt.ObjectType}, ObjectUri:{evt.ObjectUri}"); }
            } else { log.Debug($"Event skipped because type not to be processed.  Type:{evt.EventType}, ObjectType:{evt.ObjectType}, ObjectUri:{evt.ObjectUri}"); }
        }

        private void EnqueueRecord(Record record, string modelId)
        {
            var modelMessage = new ModelQueueMessage() { DbId = db.Id,  ModelId = modelId, RecordNumber = record.Number };
            var messageJson = JsonConvert.SerializeObject(modelMessage);
            PubsubMessage pubsubMessage = new PubsubMessage()
            {
                Data = ByteString.CopyFromUtf8(messageJson),
                Attributes =
                {
                    { "eventTime", DateTime.Now.ToLongTimeString() }
                }
            };
            _pub.Publish(_topic, new[] { pubsubMessage });
        }
    }
}
