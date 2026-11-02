using System;
using UnityEngine;

namespace JJB.Script.Player
{
    public class JJBPlayerRenderer : MonoBehaviour
    {
        private Transform _player;

        private void Awake()
        {
            _player = transform.parent;
        }
        
        public void FilpX(float x)
        {
            if (x > 0)
            {
                _player.eulerAngles = new Vector3(0, 0, 0);
            }
            else if (x < 0)
            {
                _player.eulerAngles = new Vector3(0, 180, 0);
            }
        }
    }
}