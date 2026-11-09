using UnityEngine;

namespace GoldCore
{
    public class Flashlight : MonoBehaviour
    {
        [SerializeField] public GameObject flashlight;
        public bool flashOn;

        // Start is called before the first frame update
        void Start()
        {
            flashlight.SetActive(false);
            flashOn = false;
        }

        // Update is called once per frame
        void Update()
        {
        if (GameInput.FlashlightPressed)
            {
                Toggle();
            }
        }

        public void turnOn()
        {
            flashlight.SetActive(true);
            flashOn = true;
        }

        public void turnOff()
        {
            flashlight.SetActive(false);
            flashOn = false;
        }

        public void Toggle()
        {
            if (flashOn)
                turnOff();
            else
                turnOn();
        }
     }
}
