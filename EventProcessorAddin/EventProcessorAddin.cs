using HP.HPTRIM.SDK;
using log4net.Config;

namespace com.cmramble.ml.casefiles
{
    public class EventProcessorAddin : HP.HPTRIM.SDK.TrimEventProcessorAddIn
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private ModelOperator modelOperator = null;

        public EventProcessorAddin()
        {
            XmlConfigurator.Configure();
        }

        public override void ProcessEvent(Database db, TrimEvent evt)
        {
            if (modelOperator == null) modelOperator = new ModelOperator(db);
            modelOperator.ProcessEvent(db, evt);
        }
    }
}
