using Sirenix.OdinInspector;

public abstract class BaseScriptObject<T> : SerializedScriptableObject where T : class
{
    public T Value;
}
