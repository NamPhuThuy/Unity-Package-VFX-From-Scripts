using UnityEngine;

namespace NamPhuThuy.AnimateWithScripts
{
    public abstract class ObjActiveAuto : MonoBehaviour
    {
        [Header("Flags")]
        [SerializeField] StartIn startIn = StartIn.ON_ENABLE;

        protected enum StartIn
        {
            NOT_AUTO_PLAY = 0,
            ON_ENABLE = 1,
            START = 2,
            
        }

        #region MonoBehaviour

        protected virtual void Start()
        {
            if (startIn == StartIn.START)
            {
                Play();
            }
        }
        
        protected virtual void OnDestroy()
        {
            if (startIn == StartIn.START)
            {
                Stop();
            }
        }

        protected virtual void OnEnable()
        {
            if (startIn == StartIn.ON_ENABLE)
            {
                Play();
            }
        }
        
        protected virtual void OnDisable()
        {
            if (startIn == StartIn.ON_ENABLE)
            {
                Stop();
            }
        }

        #endregion

        public abstract void Play();

        public abstract void Stop();
    }

}