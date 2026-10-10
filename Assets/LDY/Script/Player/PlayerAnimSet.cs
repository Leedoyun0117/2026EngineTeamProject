using System;
using System.Collections.Generic;
using UnityEngine;

namespace LDY.Script
{
    [CreateAssetMenu(menuName = "LDY/Player Anim Set", fileName = "PlayerAnimSet")]
    public class PlayerAnimSet : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public PlayerAnimId id;
            public PlayerAnimClip clip;
        }

        [SerializeField] private Entry[] clips = Array.Empty<Entry>();
        [Tooltip("Jump/Fall 클립이 없을 때 고정해서 보여줄 Run 클립의 프레임 번호")]
        [SerializeField, Min(0)] private int airHoldRunFrame;

        [Tooltip("Alt 중 고정해서 보여줄 스프라이트(눈 감은 모습)")]
        [SerializeField] private Sprite altSprite;

        private Dictionary<PlayerAnimId, PlayerAnimClip> _lookup;
        private PlayerAnimClip _airHold;

        public Sprite AltSprite
        {
            get => altSprite;
            set => altSprite = value;
        }

        public Entry[] Clips
        {
            get => clips;
            set
            {
                clips = value;
                _lookup = null;
                _airHold = null;
            }
        }

        public PlayerAnimClip Resolve(PlayerAnimId id)
        {
            if (TryGet(id, out PlayerAnimClip clip))
                return clip;

            if ((id == PlayerAnimId.Jump || id == PlayerAnimId.Fall) && TryGet(PlayerAnimId.Run, out PlayerAnimClip run))
                return _airHold ??= PlayerAnimClip.Hold(run.frames[Mathf.Min(airHoldRunFrame, run.frames.Length - 1)]);

            return null;
        }

        public Sprite FirstFrame(PlayerAnimId id)
        {
            return TryGet(id, out PlayerAnimClip clip) ? clip.frames[0] : null;
        }

        private bool TryGet(PlayerAnimId id, out PlayerAnimClip clip)
        {
            if (_lookup == null)
            {
                _lookup = new Dictionary<PlayerAnimId, PlayerAnimClip>();
                foreach (Entry entry in clips)
                {
                    if (entry.clip != null && entry.clip.IsValid)
                        _lookup[entry.id] = entry.clip;
                }
            }

            return _lookup.TryGetValue(id, out clip);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _lookup = null;
            _airHold = null;
        }
#endif
    }
}
