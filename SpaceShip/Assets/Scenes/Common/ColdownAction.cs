using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scenes.Common
{
    class ColdownAction : MonoBehaviour
    {

        private double ColdownTime;
        private bool ColdownFlag;
        private double ActionTime;
        private bool ActionFlag;
        private double LastActionTime;
        private KeyCode FireKeyCode;
        private Slider ProgressBar;
        public List<IColdownActionListener> Listeners = new List<IColdownActionListener>();

        public static ColdownAction CreateComponent(GameObject parent, double coldownTime, double actionTime, KeyCode keyCode, Slider progressBar)
        {
            ColdownAction coldownAction = parent.AddComponent(typeof(ColdownAction)) as ColdownAction;
            coldownAction.ColdownTime = coldownTime;
            coldownAction.ActionTime = actionTime;
            coldownAction.FireKeyCode = keyCode;
            coldownAction.ProgressBar = progressBar;
            return coldownAction;
        }

        public void Start()
        {

        }

        public void Update()
        {

            if (this.ActionFlag && Time.time - this.LastActionTime > this.ActionTime)
            {
                this.ActionFlag = false;
                this.ColdownFlag = true;
            }
            if (Input.GetKeyDown(this.FireKeyCode) && !this.ActionFlag && !this.ColdownFlag)
            {
                this.ActionFlag = true;
                this.LastActionTime = Time.time;
                if (this.ProgressBar != null) this.ProgressBar.value = 0;
                this.Listeners.ForEach(listener => listener.OnActionStart());
            }
            if (this.ColdownFlag)
            {
                if (this.ProgressBar != null)
                    this.ProgressBar.value = (float)((Time.time - this.LastActionTime) / this.ColdownTime);
                if (Time.time - this.LastActionTime > this.ColdownTime)
                {
                    this.ColdownFlag = false;
                    if (this.ProgressBar != null) this.ProgressBar.value = 1;
                }
            }
        }

        public bool DoingAction => this.ActionFlag;

    }

    public interface IColdownActionListener
    {
        void OnActionStart();
    }

}
