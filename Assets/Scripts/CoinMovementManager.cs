using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts
{
    public class CoinMovementManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private float _startDragTime;
        private Vector2 _startPosition;

        private bool _moving;

        public bool Completed;

        void Update()
        {
            if (_moving)
            {
                var rb = GetComponent<Rigidbody2D>();

                if (rb.velocity == Vector2.zero)
                {
                    _moving = false;
                    Completed = true;
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_moving)
            {
                return;
            }

            _startDragTime = Time.time;
            _startPosition = eventData.pressPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_moving)
            {
                return;
            }

            var distance = Vector2.Distance(_startPosition, eventData.position);

            // Allow for a little movement if the player is holding the coin before flicking
            if (distance < 25)
            {
                _startDragTime = Time.time;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_moving)
            {
                return;
            }

            var endDragTime = Time.time;
            var endPosition = eventData.position;

            var duration = endDragTime - _startDragTime;

            var direction = endPosition - _startPosition;

            var distance = direction.magnitude;

            var power = distance / duration;

            power = Mathf.Clamp(power, 0f, 0.015f);

            var rb = eventData.pointerDrag.GetComponent<Rigidbody2D>();

            rb.velocity = direction * power;

            _moving = true;
        }
    }
}
