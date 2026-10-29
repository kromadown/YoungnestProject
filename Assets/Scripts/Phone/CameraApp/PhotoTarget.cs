using System;
using UnityEngine;

namespace Phone.CameraApp
{
    public class PhotoTarget : MonoBehaviour
    { 
        public event Action Photographed;

        public void OnPhotographed()
        {
            Debug.Log($"Took a picture of {name}");
            Photographed?.Invoke();
        }
    }
}