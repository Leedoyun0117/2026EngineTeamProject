using System;
using UnityEngine;

namespace HES.Script.Object {
    public class Semaphore : ButtonParent, ISemaphore {
        //신호기니까 신호를 보내야겠지? 그럼 신호를 받는애는 이새끼 자식으로 들어가야하나 글쎄 아니면 전체 object순회하면서 찾아야할텐데 어떤식으로 만들어야할까 event로 만들것같은데
        public event Action OnResponse;
        protected override void OnTriggerEnter2D(Collider2D other) {
            base.OnTriggerEnter2D(other);
            OnResponse?.Invoke();
        }
    }
}