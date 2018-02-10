using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts
{
    public class CoinMovementManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private float _startDragTime;
        private float _endDragTime;

        private Vector2 _startPosition;
        private Vector2 _endPosition;

        private bool _moving;

        public bool Completed = false;

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
            var distance = Vector2.Distance(_startPosition, eventData.position);

            // Allow for a little movement if the player is holding the coin before flicking
            if (distance < 25)
            {
                _startDragTime = Time.time;
            }
            else
            {
                Debug.Log("Far enough");
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_moving)
            {
                return;
            }

            _endDragTime = Time.time;
            _endPosition = eventData.position;


            var duration = _endDragTime - _startDragTime;

            var direction = _endPosition - _startPosition;

            var distance = direction.magnitude;

            var power = distance / duration;

            power = Mathf.Clamp(power, 0f, 0.015f);


            var rb = eventData.pointerDrag.GetComponent<Rigidbody2D>();

            //var v2 = _endPosition - _startPosition;
            //var relativev2 = v2 / (float) ((_endDragTime - _startDragTime).TotalMilliseconds * 0.005f);
            
            //rb.velocity = relativev2 * 0.05f;

            rb.velocity = direction * power;

            _moving = true;
        }
    }
}
