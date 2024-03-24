﻿using System ;
using System.Collections.Generic ;
using UnityEngine ;
using UnityEngine.Events ;

namespace GG.Infrastructure.Utils.Swipe {
   [Serializable]
   public class SwipeListenerEvent : UnityEvent<string> {

   }

    public class SwipeListener : MonoBehaviour
    {
        public UnityEvent OnSwipeCancelled;

        public SwipeListenerEvent OnSwipe;

        [SerializeField]
        private float sensitivity = 75f;

        [SerializeField]
        private SwipeDetectionMode _swipeDetectionMode = SwipeDetectionMode.EightSides;

        private float _minMoveDistance;

        public Vector3 _swipePoint;

        public Vector3 _offset;

        private VectorToDirection _directions;

        public float Sensitivity
        {
            get { return sensitivity; }
            set
            {
                sensitivity = value;
                UpdateSensetivity();
            }
        }

        public SwipeDetectionMode SwipeDetectionMode { get { return _swipeDetectionMode; } set { _swipeDetectionMode = value; } }

        public void SetDetectionMode(List<DirectionId> directions)
        {
            _directions = new VectorToDirection(directions);
        }

        private void Start()
        {
            UpdateSensetivity();

            if (SwipeDetectionMode != SwipeDetectionMode.Custom)
            {
                SetDetectionMode(DirectionPresets.GetPresetByMode(SwipeDetectionMode));
            }
        }

        private void UpdateSensetivity()
        {
            _minMoveDistance = Screen.height / sensitivity;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                InitSwipe();
            }

            if (Input.GetMouseButton(0))
            {
                CheckSwipe();
            }

            CheckSwipeCancellation();
        }

        private void CheckSwipeCancellation() {
            if (Input.GetMouseButtonUp(0))
            {
                OnSwipeCancelled?.Invoke();
            }
        }
        public bool HasSwipedEnough()
        {
            if (!Input.GetMouseButtonUp(0)) return false;
            _offset = Input.mousePosition - _swipePoint;
            return _offset.y >= _minMoveDistance;
        }
        private void InitSwipe(){
            SampleSwipeStart();
        }

        private void CheckSwipe()
        {
            _offset = Input.mousePosition - _swipePoint;
            if (_offset.magnitude >= _minMoveDistance)
            {
                if (OnSwipe != null)
                    OnSwipe.Invoke(_directions.GetSwipeId(_offset));
                
                SampleSwipeStart();
            }
        }

        private void SampleSwipeStart () {
            _swipePoint = Input.mousePosition ;
            _offset = Vector3.zero ;
        }
   }
}