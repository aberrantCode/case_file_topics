using HP.HPTRIM.SDK;

namespace com.cmramble.ml.casefiles
{
    public class EventProcessorAddin : HP.HPTRIM.SDK.TrimEventProcessorAddIn
    {
        public override void ProcessEvent(Database db, TrimEvent evt)
        {
            // if this is a record that was just added
            if ( evt.EventType == Events.ObjectAdded && evt.ObjectType == BaseObjectTypes.Record )
            {
                // check that this record was added into a folder that has an ML operator attached
                Record record = new Record(db, evt.ObjectUri);
                if (record != null )
                {
                    Record container = record.Container;
                    // check that there is a container
                    if (container != null)
                    {
                        Classification classification = container.Classification;
                        // check that there is a classification attached
                        if ( classification != null) { 
                            FieldDefinition modelOperatorField = new FieldDefinition(db, "mlmodel");
                            // check that there is a model operator definition
                            if (modelOperatorField != null)
                            {
                                string modelId = classification.GetFieldValueAsString(modelOperatorField, StringDisplayType.Default, false);
                                // check that there is some value for this classification's mlmodel property
                                if (!string.IsNullOrWhiteSpace(modelId))
                                {
                                    // okay we have something we can work with.... let's ship it off to the pubsub topic

                                }
                            }
                        }
                } 
                }
            }
        }
    }
}
