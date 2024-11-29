using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Singleton
    {
        get => _singleton;
        set
        {
            if (value == null)
                _singleton = null;
            else if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Destroy(value);
                Debug.LogError($"There should only ever be one instance of {nameof(CameraFollow)}!");
            }
        }
    }
    private static CameraFollow _singleton;

    private Transform target;
    public Vector3 followOffset;

    private void Awake()
    {
        Singleton = this;
    }

    private void OnDestroy()
    {
        if (Singleton == this)
            Singleton = null;
    }

    private void LateUpdate()
    {
        //var targetRotation = target.rotation;
        //targetRotation.x = 0; targetRotation.z = 0;
        //Debug.Log("CameraFollow targetRotation>>" + targetRotation);

        if (target != null)
        {
             transform.SetPositionAndRotation(target.position + followOffset, Quaternion.identity);
            //transform.SetPositionAndRotation(target.position + followOffset, Quaternion.Euler(new Vector3(-1, 0, -1)));
        }
    }

    public void SetTarget(Transform newTarget)
    {
        Debug.Log("CameraFollow SetTarget>>" + newTarget);
        target = newTarget;
    }
}
