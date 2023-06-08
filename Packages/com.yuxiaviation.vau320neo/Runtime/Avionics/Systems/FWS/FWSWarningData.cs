using UdonSharp;

namespace A320VAU.FWS
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public partial class FWSWarningData : UdonSharpBehaviour
    {
        private FWS FWS;
        private bool _hasWarningVisibleChange = false;
        private bool _hasWarningDataVisibleChange = false;

        private FWSWarningMessageData[] _fwsWarningMessageData;

        private void Start()
        {
            _fwsWarningMessageData = GetComponentsInChildren<FWSWarningMessageData>();

            SetupEngine();
            SetupConfigMemo();
            SetupGear();
            SetupMemo();
            SetupConfig();
        }

        private FWSWarningMessageData GetWarningMessageData(string id)
        {
            foreach (var data in _fwsWarningMessageData)
            {
                if (data.Id != id) continue;
                return data;
            }

            return null;
        }

        public void Monitor(FWS fws)
        {
            _hasWarningVisibleChange = false;
            _hasWarningDataVisibleChange = false;
            FWS = fws;

            MonitorEngine();
            MonitorConfigMemo();
            MonitorGear();
            MonitorMemo();
            MonitorConfig();

            fws._hasWarningDataVisableChange = _hasWarningDataVisibleChange;
            fws._hasWarningVisableChange = _hasWarningVisibleChange;
        }

        private void SetWarnVisible(ref bool isVisible, bool newValue, bool isWarnData = false)
        {
            if (isVisible == newValue) return;
            if (isWarnData) _hasWarningDataVisibleChange = true;
            
            isVisible = newValue;
            _hasWarningVisibleChange = true;
        }
    }
}