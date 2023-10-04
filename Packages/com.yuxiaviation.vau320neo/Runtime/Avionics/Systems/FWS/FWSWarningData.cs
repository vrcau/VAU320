using UdonSharp;

namespace A320VAU.FWS {
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public partial class FWSWarningData : UdonSharpBehaviour {
        private FWSWarningMessageData[] _fwsWarningMessageData;
        private bool _hasWarningDataVisibleChange;
        private bool _hasWarningVisibleChange;
        private FWS FWS;

        private void Start() {
            _fwsWarningMessageData = GetComponentsInChildren<FWSWarningMessageData>();

            SetupEngine();
            SetupConfigMemo();
            SetupGear();
            SetupMemo();
            SetupConfig();
            SetupSpeed();
        }

        private FWSWarningMessageData GetWarningMessageData(string id) {
            foreach (var data in _fwsWarningMessageData) {
                if (data.Id != id) continue;
                return data;
            }

            return null;
        }

        public void Monitor(FWS fws) {
            _hasWarningVisibleChange = false;
            _hasWarningDataVisibleChange = false;
            FWS = fws;

            MonitorEngine();
            MonitorConfigMemo();
            MonitorGear();
            MonitorMemo();
            MonitorConfig();
            MonitorSpeed();

            fws._hasWarningDataVisibleChange = _hasWarningDataVisibleChange;
            fws._hasWarningVisibleChange = _hasWarningVisibleChange;
        }

        private void SetWarnVisible(ref bool isVisible, bool newValue, bool isWarnData = false) {
            if (isVisible == newValue) return;
            if (isWarnData) _hasWarningDataVisibleChange = true;

            isVisible = newValue;
            _hasWarningVisibleChange = true;
        }
    }
}