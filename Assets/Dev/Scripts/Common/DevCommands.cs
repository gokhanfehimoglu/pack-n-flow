using UnityEngine;

namespace PackNFlow
{
    public class DevCommands : MonoBehaviour
    {
        private const KeyCode WIN_KEY = KeyCode.O;
        private const KeyCode FAIL_KEY = KeyCode.F;

        private void Update()
        {
            if (Input.GetKeyDown(WIN_KEY))
            {
                GameBootstrap.Instance.ForceLevelEnd(true);
            }
            else if (Input.GetKeyDown(FAIL_KEY))
            {
                GameBootstrap.Instance.ForceLevelEnd(false);
            }
        }
    }
}
