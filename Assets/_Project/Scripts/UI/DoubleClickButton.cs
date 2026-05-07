using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DiamondMind.Prototypes.Tools
{
    [RequireComponent(typeof(Button))]
    public class DoubleClickButton : MonoBehaviour
    {
        [SerializeField] private float doubleClickTime = 0.3f;
        [SerializeField] private UnityEvent onDoubleClick;

        private float lastClickTime;

        private void OnEnable()
        {
            GetComponent<Button>().onClick.AddListener(OnButtonClick);
        }

        private void OnDisable()
        {
            GetComponent<Button>().onClick.RemoveListener(OnButtonClick);
        }

        public void OnButtonClick()
        {
            if (Time.time - lastClickTime <= doubleClickTime)
            {
                onDoubleClick.Invoke();
                lastClickTime = 0f;
            }
            else
            {
                lastClickTime = Time.time;
            }
        }
    }
}
