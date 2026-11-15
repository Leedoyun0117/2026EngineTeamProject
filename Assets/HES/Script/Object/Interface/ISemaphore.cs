using System;

namespace HES.Script.Object {
    public interface ISemaphore {
        event Action OnResponse;
    }
}