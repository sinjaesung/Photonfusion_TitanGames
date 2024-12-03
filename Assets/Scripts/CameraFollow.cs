using Fusion;
using UnityEngine;
using Cinemachine;
using UnityEngine.Windows;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Singleton
    {
        get => _singleton;
       /* set
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
        }*/
    }
    private static CameraFollow _singleton;

    private Transform target;
    public Vector3 followOffset;

    //Camera Variables
    public float CameraHeight = 1.75f, CameraMaxDistance = 25;
    float CameraMaxTilt = 90;
    [Range(0, 4)]
    public float CameraSpeed = 2;
    public float currentPan, currentTilt = 10, currentDistance = 5;
    [HideInInspector]
    public bool autoRunReset = false;

    //CameraSmoothing
    [SerializeField]
    float panAngle, panOffset;
    bool camXAdjust, camYAdjust;
    float rotationXCushion = 3, rotationXSpeed = 0;
    float yRotMin = 0, yRotMax = 20, rotationYSpeed = 0;

    //CamState
    public CameraState cameraState = CameraState.CameraNone;

    //Options
    [Range(0.25f, 1.75f)]
    public float cameraAdjustSpeed = 1;
    public CameraMoveState camMoveState = CameraMoveState.OnlyWhileMoving;

    //Collision
    public bool collisionDebug;
    public float collisionCushion = 0.35f;
    public float clipCushion = 1.5f;
    public int rayGridX = 9, rayGridY = 5;
    float adjustDistance;
    public LayerMask collisionMask;
    Vector3[] camClip, clipDirection, playerClip, rayColOrigin, rayColPoint;
    bool[] rayColHit;
    Ray camRay;
    RaycastHit camRayHit;

    //References
    Player player;
    public Transform tilt;
    public Camera mainCam;

    #region singleton
    private void Awake()
    {
        if (_singleton != null && _singleton != this)
        {
            //다른 씬 갔다가 왔을때 awake또 실행되는데, 이때 awake로 새로 만들어지는데 새로만들어지는것만 삭제
            Debug.Log("싱글톤 CameraFollow 한번이상 실행된 경우! [[CameraFollow Awake 유일실행]]");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("싱글톤 CameraFollow 최초로 실행된 경우! [[CameraFollow Awake 유일실행]]");
            _singleton = this;
            DontDestroyOnLoad(gameObject);//다른씬으로 갈때 현재 오브젝트 삭제하지않는다
        }
    }
    #endregion
    private void Start()
    {
    }
    private void OnDestroy()
    {
        if (_singleton == this)
            _singleton = null;
    }

    private void LateUpdate()
    {
        //var targetRotation = target.rotation;
        //targetRotation.x = 0; targetRotation.z = 0;
        //Debug.Log("CameraFollow targetRotation>>" + targetRotation);

        if (target != null)
        {
            //transform.SetPositionAndRotation(target.position + followOffset, Quaternion.identity);
            //transform.SetPositionAndRotation(target.position + followOffset, Quaternion.Euler(new Vector3(-1, 0, -1)));
            //CinemachineFreeLook cinemachine = GetComponent<CinemachineFreeLook>();
            //cinemachine.Follow = target;
            //cinemachine.LookAt = target;
            //CinemachineVirtualCamera followCam = FindObjectOfType<Cinemachine.CinemachineVirtualCamera>();
            //Debug.Log("followCam>>" + followCam.transform.name);
            //followCam.Follow = target;
            //followCam.LookAt = target;
        }

        /*panAngle = Vector3.SignedAngle(transform.forward, player.transform.forward, Vector3.up);
        
        switch (camMoveState)
        {
            case CameraMoveState.OnlyWhileMoving:
                if (player.inputManager.accumulatedInput.Direction.magnitude > 0 || player.rotation != 0)
                {
                    CameraXAdjust();
                    CameraYAdjust();
                }
                break;
            case CameraMoveState.OnlyHorizontalWhileMoving:
                if (player.inputManager.accumulatedInput.Direction.magnitude > 0 || player.rotation != 0)
                    CameraXAdjust();
                break;
            case CameraMoveState.AlwaysAdjust:
                CameraXAdjust();
                CameraYAdjust();
                break;
            case CameraMoveState.NeverAdjust:
                CameraNeverAdjust();
                break;
        }*/
        CameraTransforms();
    }

    public void SetTarget(Transform newTarget)
    {
        Debug.Log("CameraFollow SetTarget>>" + newTarget);
        target = newTarget;
        player = target.GetComponent<Player>();

        player.mainCam = this;
        transform.position = player.transform.position + Vector3.up * CameraHeight;
        //transform.rotation = player.transform.rotation;

        //tilt.eulerAngles = new Vector3(currentTilt, transform.eulerAngles.y, transform.eulerAngles.z);
        mainCam.transform.position += tilt.forward * -currentDistance;

        CameraClipInfo();
    }

    void Update()
    {
        //if (GetInput(out NetInput input))
        //{
        if (player)
        {
            if (!player.inputManager.IsLeftMouseButtonDown && !player.inputManager.IsRightMouseButtonDown)//if no mouse button is pressed
            {
                cameraState = CameraState.CameraNone;
            }
            else if (player.inputManager.IsLeftMouseButtonDown && !player.inputManager.IsRightMouseButtonDown)//if left mouse button is pressed
            {
                cameraState = CameraState.CameraRotate;
            }
        }
       
        /*else if (!player.inputManager.IsLeftMouseButtonDown && player.inputManager.IsRightMouseButtonDown)//if right mouse button is pressed
        {
            cameraState = CameraState.CameraSteer;
        }*/

        //}
        /* if (!Input.GetKey(leftMouse) && !Input.GetKey(rightMouse) && !Input.GetKey(middleMouse)) //if no mouse button is pressed
         cameraState = CameraState.CameraNone;
     else if (Input.GetKey(leftMouse) && !Input.GetKey(rightMouse) && !Input.GetKey(middleMouse)) //if left mouse button is pressed
         cameraState = CameraState.CameraRotate;
     else if (!Input.GetKey(leftMouse) && Input.GetKey(rightMouse) && !Input.GetKey(middleMouse)) //if right mouse button is pressed
         cameraState = CameraState.CameraSteer;
     else if ((Input.GetKey(leftMouse) && Input.GetKey(rightMouse)) || Input.GetKey(middleMouse)) //if left and right mouse button or middle mouse button is pressed
         cameraState = CameraState.CameraRun;*/

        if (rayColOrigin != null)
        {
            if (rayGridX * rayGridY != rayColOrigin.Length)
                CameraClipInfo();
        }
           

        CameraCollisions();
        //CameraInputs();
        CameraWheeling();
    }
    void CameraClipInfo()
    {
        camClip = new Vector3[4];

        mainCam.CalculateFrustumCorners(new Rect(0, 0, 1, 1), mainCam.nearClipPlane, Camera.MonoOrStereoscopicEye.Mono, camClip);
        //camClip은 이차원투영좌표계 인듯하다.

        clipDirection = new Vector3[4];
        playerClip = new Vector3[4];

        int rays = rayGridX * rayGridY;

        rayColOrigin = new Vector3[rays];
        rayColPoint = new Vector3[rays];
        rayColHit = new bool[rays];
    }
    void CameraCollisions()
    {
        float camDistance = currentDistance + collisionCushion;

        for (int i = 0; i < camClip.Length; i++)
        {
            Vector3 clipPoint = mainCam.transform.up * camClip[i].y + mainCam.transform.right * camClip[i].x;
            clipPoint *= clipCushion;
            clipPoint += mainCam.transform.forward * camClip[i].z;
            clipPoint += transform.position - (tilt.forward * CameraMaxDistance);

            Vector3 playerPoint = mainCam.transform.up * camClip[i].y + mainCam.transform.right * camClip[i].x;
            playerPoint += transform.position;

            clipDirection[i] = (clipPoint - playerPoint).normalized;
            playerClip[i] = playerPoint;

            Debug.DrawLine(playerClip[i], playerClip[i] + clipDirection[i] * camDistance, Color.magenta);
        }

        int currentRay = 0;
        bool isColliding = false;

        float rayX = rayGridX - 1;
        float rayY = rayGridY - 1;

        for (int x = 0; x < rayGridX; x++)
        {
            Vector3 CU_Point = Vector3.Lerp(clipDirection[1], clipDirection[2], x / rayX);
            Vector3 CL_Point = Vector3.Lerp(clipDirection[0], clipDirection[3], x / rayX);

            Vector3 PU_Point = Vector3.Lerp(playerClip[1], playerClip[2], x / rayX);
            Vector3 PL_Point = Vector3.Lerp(playerClip[0], playerClip[3], x / rayX);
            for (int y = 0; y < rayGridY; y++)
            {
                camRay.origin = Vector3.Lerp(PU_Point, PL_Point, y / rayY);
                camRay.direction = Vector3.Lerp(CU_Point, CL_Point, y / rayY);
                rayColOrigin[currentRay] = camRay.origin;

                if (Physics.Raycast(camRay, out camRayHit, camDistance, collisionMask))
                {
                    isColliding = true;
                    rayColHit[currentRay] = true;
                    rayColPoint[currentRay] = camRayHit.point;

                    if (collisionDebug)
                    {
                        Debug.DrawLine(camRay.origin, camRayHit.point, Color.cyan);
                        Debug.DrawLine(camRayHit.point, camRay.origin + camRay.direction * camDistance, Color.magenta);
                    }
                }
                else
                {
                    if (collisionDebug)
                        Debug.DrawLine(camRay.origin, camRay.origin + camRay.direction * camDistance, Color.cyan);
                }

                currentRay++;
            }
        }

        if (isColliding)
        {
            float minRayDistance = float.MaxValue;
            currentRay = 0;

            for (int i = 0; i < rayColHit.Length; i++)
            {
                if (rayColHit[i])
                {
                    float colDistance = Vector3.Distance(rayColOrigin[i], rayColPoint[i]);

                    if (colDistance < minRayDistance)
                    {
                        minRayDistance = colDistance;
                        currentRay = i;
                    }
                }
            }

            Vector3 clipCenter = transform.position - (tilt.forward * currentDistance);

            adjustDistance = Vector3.Dot(-mainCam.transform.forward, clipCenter - rayColPoint[currentRay]);
            adjustDistance = currentDistance - (adjustDistance + collisionCushion);
            adjustDistance = Mathf.Clamp(adjustDistance, 0, CameraMaxDistance);
        }
        else
            adjustDistance = currentDistance;
        /*if(Physics.Raycast(camRay,out camRayHit, camDistance, collisionMask))
        {
            //collisionMask 집합에 속한 투사체레이어오브젝트에 투사된경우엔 이 부분 실행
            adjustDistance = Vector3.Distance(camRay.origin, camRayHit.point) - collisionCushion;
            Debug.Log("카메라충돌체가 있는경우:" + adjustDistance);
        }
        else
        {
            //collisionMask 집합에 속하지 않은 투사체레이어오브젝트에 투사된경우엔 이 부분 실행 또는 카메라충돌체가 없는경우
            adjustDistance = currentDistance;
            Debug.Log("카메라 충돌체가 없는경우" + adjustDistance);
        }*/
    }
    void CameraInputs()
    {
        //if (GetInput(out NetInput input))
        //{
            if (cameraState != CameraState.CameraNone)
            {
                if (!camYAdjust && (camMoveState == CameraMoveState.AlwaysAdjust) || camMoveState == CameraMoveState.OnlyWhileMoving)
                {
                    //camYAdjust는 웬만한 카메라입력에서 true로 설정 허용
                    camYAdjust = true;
                }
                if (cameraState == CameraState.CameraRotate)
                {
                    //좌측마우스 조작때에만 이후에 camXAdjust 처리하도록 설정
                    if (!camXAdjust && camMoveState != CameraMoveState.NeverAdjust)
                        camXAdjust = true;

                   /* if (player.steer)
                        player.steer = false;*/

                    currentPan += player.NetInput.LookDelta.x * CameraSpeed;

                    /*if (!player.steer)
                    {
                        Vector3 playerReset = player.transform.eulerAngles;
                        playerReset.y = transform.eulerAngles.y; //플레이어y축회전량을 카메라회전y량이랑 동일하게처리

                        player.transform.eulerAngles = playerReset;
                        //카메라run(가운데마우스)나 우측 마우스 조작시에 캐릭터steer true처리
                        player.steer = true;
                        //카메라조작에서 steer를 먼저 설정을해두고,캐릭터의 회전초기값을 현재 카메라y축 회전량으로 맞춰둔다.
                        //이후에 player.steer로 인해 캐릭터에서 우측마우스조정으로 캐릭터y축회전이 이뤄나면(마우스우측조작중에 계속 해당한다)
                    }*/
                }
                /*else if (cameraState == CameraState.CameraSteer || cameraState == CameraState.CameraRun)
                {
                    if (!player.steer)
                    {
                        Vector3 playerReset = player.transform.eulerAngles;
                        playerReset.y = transform.eulerAngles.y; //플레이어y축회전량을 카메라회전y량이랑 동일하게처리

                        player.transform.eulerAngles = playerReset;
                        //카메라run(가운데마우스)나 우측 마우스 조작시에 캐릭터steer true처리
                        player.steer = true;
                        //카메라조작에서 steer를 먼저 설정을해두고,캐릭터의 회전초기값을 현재 카메라y축 회전량으로 맞춰둔다.
                        //이후에 player.steer로 인해 캐릭터에서 우측마우스조정으로 캐릭터y축회전이 이뤄나면(마우스우측조작중에 계속 해당한다)
                    }
                }*/

                //currentTilt -= Input.GetAxis("Mouse Y") * CameraSpeed;
                //currentTilt -= player.NetInput.LookDelta.y * CameraSpeed;
                //currentTilt = Mathf.Clamp(currentTilt, -CameraMaxTilt, CameraMaxTilt);

                // Debug.Log("cameraInputs currentPan,currentTilt:" + currentPan + "," + currentTilt);
            }
             else
             {
                 if (player.steer)
                     player.steer = false;
             }

            //currentDistance -= Input.GetAxis("Mouse ScrollWheel") * 2;
            float scrollDelta = player.inputManager.ScrollWheelDelta *2;
            currentDistance -= scrollDelta;
            currentDistance = Mathf.Clamp(currentDistance, 0, CameraMaxDistance);
        //}
    }
    void CameraWheeling()
    {
        float scrollDelta = player.inputManager.ScrollWheelDelta * 2;
        currentDistance -= scrollDelta;
        currentDistance = Mathf.Clamp(currentDistance, 0, CameraMaxDistance);
    }
    void CameraXAdjust()
    {
        if (cameraState != CameraState.CameraRotate)
        {
            if (camXAdjust)
            {
                //좌측마우스로 카메라 조작한 이후에 카메라y축(x방향)회전 조정하는부분
                rotationXSpeed += Time.deltaTime * cameraAdjustSpeed;

                if (Mathf.Abs(panAngle) > rotationXCushion)
                {
                    //좌측마우스로 카메라조정이후에 키보드조작때 처리한다.(캐릭터는 회전안되고 카메라만 회전된상태이다)
                    //이 때 카메라의 회전y량은 다시 panAngle로 캐릭터에대해 이동량만큼 다시 더해주기에 다시 캐릭터y축회전량과 같은 상태로 초기화됨!
                    //Debug.Log("panAngle이동량 빠르게 이동되어 트랜지션처리" + currentPan + "," + currentPan + panAngle);
                    currentPan = Mathf.Lerp(currentPan, currentPan + panAngle, rotationXSpeed);
                }
                else
                {
                    //Debug.Log("rotateXCushion량 이하로 이동되어 camAdjust미처리");
                    camXAdjust = false; //x방향 카메라회전 조정하지 않았다는 뜻이다!
                }
            }
            else
            {
                if (rotationXSpeed > 0)
                    rotationXSpeed = 0;
                //우측마우스,cameraRun,키보드조작등 하고있는 상황일때.키보드조작때도 키보드조작에 의한 캐릭터y축회전량만큼,카메라도 같이 따라감.
                //Debug.Log("camXAdjust==false인상황에 카메라currentPan을 캐릭터y축회전량으로조정");
                currentPan = player.transform.eulerAngles.y; //결과적으론 우측마우스조정때는 캐릭터의 y축회전량을 카메라가 동일하게 따라가는것과같다.
            }
            Debug.Log("CamXAdjust>>" + panAngle+">"+ currentPan);

        }
    }
    void CameraYAdjust()
    {
        if (cameraState == CameraState.CameraNone)
        {
            //Debug.Log("cameraYAdjust는 CameraNone상태일때만(좌,우,카메라런이 아닌 상태)에만 적용:키보드조작시");
            if (camYAdjust)
            {

                rotationYSpeed += (Time.deltaTime / 2) * cameraAdjustSpeed;

                if (currentTilt >= yRotMax || currentTilt <= yRotMin)
                {
                    //Debug.Log("currentTIlt범위가 yRotmin or yRotmax 범위를 넘을때 최대yRotMax/2값으로 임의조정" + currentTilt);
                    currentTilt = Mathf.Lerp(currentTilt, yRotMax / 2, rotationYSpeed);
                }
                else if (currentTilt < yRotMax && currentTilt > yRotMin)
                {
                    //Debug.Log("currentTIlt범위가 yRotmin~yRotMax사이에 있을때: 별다른currentTilt조정처리안함" + currentTilt);
                    camYAdjust = false;
                }
            }
            else
            {
                //Debug.Log("camYAdjust false인경우 rotationYSpeed값 초기화조정");
                if (rotationYSpeed > 0)
                    rotationYSpeed = 0;
            }
            Debug.Log("CamYAdjust>>"+currentTilt);

        }
    }
    void CameraNeverAdjust()
    {
        switch (cameraState)
        {
            case CameraState.CameraSteer:
            case CameraState.CameraRun:
                if (panOffset != 0)
                    panOffset = 0;

                currentPan = player.transform.eulerAngles.y;
                //Debug.Log("우측마우스제어 or cameraRun제어시엔 currentPan 캐릭터y축 회전량과 동일하게 처리:(panOffset0처리,캐릭터&카메라y축동일화)" + currentPan);
                break;
            case CameraState.CameraNone:
                currentPan = player.transform.eulerAngles.y - panOffset;
                //Debug.Log("아무런 카메라 미조작시(키보드조작) 캐릭터y축회전값 panOffset값 제어 panOffset,currentPan check:" + panOffset+","+ currentPan);
                break;
            case CameraState.CameraRotate:
                panOffset = panAngle;
                //Debug.Log("좌측마우스 카메라만 회전시에 panOffset check(카메라y축회전량,캐릭터y축회전량offset차이값구해둠)" + panOffset);
                break;
        }
    }

    void CameraTransforms()
    {
        /*if (cameraState == CameraState.CameraNone)
            currentTilt = 10;*/

        if (cameraState == CameraState.CameraRun)
        {
            player.autoRun = true;

            if (!autoRunReset)
                autoRunReset = true;
        }
        else
        {
            if (autoRunReset)
            {
                player.autoRun = false;
                autoRunReset = false;
            }
        }

        if (player)
        {
            transform.position = player.transform.position + Vector3.up * CameraHeight;
            //transform.eulerAngles = new Vector3(transform.eulerAngles.x, currentPan, transform.eulerAngles.z);
            //player.transform.eulerAngles = new Vector3(transform.eulerAngles.x, currentPan, transform.eulerAngles.z);
            //tilt.eulerAngles = new Vector3(currentTilt, tilt.eulerAngles.y, tilt.eulerAngles.z);
            mainCam.transform.position = transform.position + tilt.forward * -adjustDistance;
        }
    }
       

    public enum CameraState { CameraNone, CameraRotate, CameraSteer, CameraRun }

    public enum CameraMoveState { OnlyWhileMoving, OnlyHorizontalWhileMoving, AlwaysAdjust, NeverAdjust }
}