using UnityEngine;

namespace ClaudeOfTanks.Runtime
{
    public sealed partial class GameFlowController
    {
        private SceneStudioController _studio;

        public SceneStudioController ActiveStudio =>
            _studio;

        public void OpenStudio()
        {
            if (_studio != null ||
                _battle != null ||
                _networkBattle != null ||
                _garage == null)
            {
                return;
            }
            string vehicleId =
                SelectedVehicleId;
            string selectedMapId =
                SelectedMapId;
            DestroyGarage();
            GameObject root =
                new GameObject("SceneStudio");
            root.transform.SetParent(
                transform,
                false);
            _studio =
                root.AddComponent<
                    SceneStudioController>();
            _studio.Configure(
                _catalog,
                vehicleId,
                selectedMapId,
                CloseStudio);
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.F8))
                return;
            if (_studio != null)
                CloseStudio();
            else if (_garage != null)
                OpenStudio();
        }

        private void CloseStudio()
        {
            GameObject root =
                _studio != null
                    ? _studio.gameObject
                    : null;
            _studio = null;
            ReleaseObject(root);
            ShowGarage();
        }

        private void DisposeStudio()
        {
            if (_studio == null) return;
            GameObject root =
                _studio.gameObject;
            _studio = null;
            ReleaseObject(root);
        }
    }
}
