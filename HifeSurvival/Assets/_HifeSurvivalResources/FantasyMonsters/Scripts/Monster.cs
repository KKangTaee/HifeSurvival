using System;
using System.Collections.Generic;
using System.Linq;
using Assets.FantasyMonsters.Scripts.Tweens;
using UnityEngine;
using DG.Tweening;

namespace Assets.FantasyMonsters.Scripts
{
    /// <summary>
    /// The main script to control monsters.
    /// </summary>
    public class Monster : MonoBehaviour
    {
        public SpriteRenderer Head;
        public List<Sprite> HeadSprites;
        public Animator Animator;
        public bool Variations;
        public event Action<string> OnEvent = eventName => { };

        public Action OnDeathCompletedHandler;
        public SpriteRenderer[] _partsRendererArr;

        /// <summary>
        /// Called on Awake.
        /// </summary>
        public void Awake()
        {
            if (Variations)
            {
                var variations = GetComponents<MonsterVariation>();
                var random = UnityEngine.Random.Range(0, variations.Length + 1);

                if (random > 0)
                {
                    variations[random - 1].Apply();
                }
            }

            GetComponent<LayerManager>().SetSortingGroupOrder((int)-transform.localPosition.y);

            var stateHandler = Animator.GetBehaviours<StateHandler>().SingleOrDefault(i => i.Name == "Death");

            if (stateHandler)
            {
                stateHandler.StateExit.AddListener(() => SetHead(0));
            }

            _partsRendererArr = GetComponentsInChildren<SpriteRenderer>();
        }

        /// <summary>
        /// Set animation parameter State to control transitions. Play different state animations (except Attack).
        /// </summary>
        public void SetState(MonsterState state)
        {
            Animator.SetInteger("State", (int)state);
        }

        /// <summary>
        /// Play Attack animation.
        /// </summary>
        public void Attack()
        {
            Animator.SetTrigger("Attack");
        }

        /// <summary>
        /// Play scale spring animation.
        /// </summary>
        public virtual void Spring()
        {
            ScaleSpring.Begin(this, 1f, 1.1f, 40, 2);
        }

        // Play Die animation.
        public void Die()
        {
            SetState(MonsterState.Death);
        }

        /// <summary>
        /// Called from animation. Can be used by the game to handle animation events.
        /// </summary>
        public void Event(string eventName)
        {
            OnEvent(eventName);
        }

        /// <summary>
        /// Called from animation.
        /// </summary>
        public void SetHead(int index)
        {
            if (index != 2 && Animator.GetInteger("State") == (int)MonsterState.Death) return;

            if (index < HeadSprites.Count)
            {
                Head.sprite = HeadSprites[index];
            }
        }

        public void OnDeathCompleted()
        {
            Fade(0, 0.25f, OnDeathCompletedHandler);
        }

        public void Fade(float inEndValue, float inDuration, Action doneCallback = null)
        {
            Sequence sequence = DOTween.Sequence();

            foreach (var renderer in _partsRendererArr)
            {
                Tweener fadeTween = renderer.DOFade(inEndValue, inDuration);
                sequence.Join(fadeTween);
            }

            sequence.OnComplete(() =>
            {
                doneCallback?.Invoke();
            });
        }

        public void ResetAlphaParts()
        {
            foreach (var renderer in _partsRendererArr)
                renderer.color = Color.white;
        }
        
        public void Damage()
        {
            foreach (var renderer in _partsRendererArr)
            {
                renderer.DOColor(Color.red, 0f)
                .OnComplete(() => renderer.DOColor(Color.white, 0.2f));
            }
        }
    }
}