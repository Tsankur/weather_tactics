using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class editor_camera : MonoBehaviour
{
    // setting interface size to set the correct edition zone
    public int m_iTopInterfaceHeight = 0;
    public int m_iBottomInterfaceHeight = 0;
    public int m_iRightInterfaceWidth = 0;
    public int m_iLeftInterfaceWidth = 0;

    private Vector2 m_vMaxPosition = new Vector2(0, 0);
    public Vector2 m_vRealMaxPosition = new Vector2(0, 0);
    public Vector2 m_vRealMinPosition = new Vector2(0, 0);
    private bool m_bMoving = false;
    private Vector3 m_vLastMousePosition;
    private float m_fZoom = 150;
	// Use this for initialization
	void Start ()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, -m_fZoom);
	}
	
	// Update is called once per frame
	void Update ()
    {
#if UNITY_IPHONE || UNITY_ANDROID
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.touchCount == 2)
            {
                // Store both touches.
                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);

                if (touchZero.phase == TouchPhase.Moved || touchOne.phase == TouchPhase.Moved && !isInInterface(touchZero.position) && !isInInterface(touchOne.position))
                {
                    // Find the position in the previous frame of each touch.
                    Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                    Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                    // Find the magnitude of the vector (the distance) between the touches in each frame.
                    float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                    float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

                    // Find the difference in the distances between each frame.
                    float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
                    if (deltaMagnitudeDiff != 0.0f)
                    {
                        m_fZoom += deltaMagnitudeDiff;
                        if (m_fZoom > 300)
                        {
                            m_fZoom = 300;
                        }
                        if (m_fZoom < 50)
                        {
                            m_fZoom = 50;
                        }
                        transform.position = new Vector3(transform.position.x, transform.position.y, -m_fZoom);
                        ComputeRealMaxPositions();
                        ClampPosition();
                    }
                }
            }
            else if (Input.touchCount == 1)
            {
                Touch touchZero = Input.GetTouch(0);
                if (touchZero.phase == TouchPhase.Moved)
                {
                    m_vLastMousePosition = Input.mousePosition;
                    m_bMoving = true;
                    transform.position += new Vector3(-touchZero.deltaPosition.x, -touchZero.deltaPosition.y) / (800.0f / m_fZoom);
                    ClampPosition();
                }
            }
        }
#else
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButtonDown(1))
            {
                m_vLastMousePosition = Input.mousePosition;
                m_bMoving = true;
            }
            float fMouseWheelDelta = Input.GetAxis("Mouse ScrollWheel");
            if (fMouseWheelDelta != 0.0f)
            {
                m_fZoom -= fMouseWheelDelta * 50;
                if (m_fZoom > 300)
                {
                    m_fZoom = 300;
                }
                if (m_fZoom < 50)
                {
                    m_fZoom = 50;
                }
                transform.position = new Vector3(transform.position.x, transform.position.y, -m_fZoom);
                ComputeRealMaxPositions();
                ClampPosition();
            }
            if (m_bMoving)
            {
                transform.position += (m_vLastMousePosition - Input.mousePosition) / (800.0f / m_fZoom);
                ClampPosition();
                m_vLastMousePosition = Input.mousePosition;
            }
        }
        if (Input.GetMouseButtonUp(1))
        {
            m_bMoving = false;
        }
#endif
    }
    void ClampPosition()
    {
        float newX = transform.position.x, newY = transform.position.y;
        if (transform.position.x > m_vRealMaxPosition.x)
        {
            newX = m_vRealMaxPosition.x;
        }
        if (transform.position.y > m_vRealMaxPosition.y)
        {
            newY = m_vRealMaxPosition.y;
        }
        if (transform.position.x < m_vRealMinPosition.x)
        {
            newX = m_vRealMinPosition.x;
        }
        if (transform.position.y < m_vRealMinPosition.y)
        {
            newY = m_vRealMinPosition.y;
        }
        transform.position = new Vector3(newX, newY, -m_fZoom);
    }
    void ComputeRealMaxPositions()
    {
        float fVisionHeight = m_fZoom * Mathf.Sin(Mathf.PI / 6.0f) / Mathf.Cos(Mathf.PI / 6.0f);
        float fVisionWidth = fVisionHeight * camera.aspect;
        Vector2 vMinGamezone = new Vector2(m_iLeftInterfaceWidth / (float)Screen.width * fVisionWidth * 2, m_iBottomInterfaceHeight / (float)Screen.height * fVisionHeight * 2);
        Vector2 vMaxGamezone = new Vector2(m_iRightInterfaceWidth / (float)Screen.width * fVisionWidth * 2, m_iTopInterfaceHeight / (float)Screen.height * fVisionHeight * 2);
        m_vRealMaxPosition = new Vector2(Mathf.Max((m_vMaxPosition.x - vMinGamezone.x + vMaxGamezone.x) / 2, m_vMaxPosition.x - fVisionWidth + 5 + vMaxGamezone.x), Mathf.Max((m_vMaxPosition.y - vMinGamezone.y + vMaxGamezone.y) / 2, m_vMaxPosition.y - fVisionHeight + 5 + vMaxGamezone.y));
        m_vRealMinPosition = new Vector2(Mathf.Min((m_vMaxPosition.x - vMinGamezone.x + vMaxGamezone.x) / 2, fVisionWidth - 5 - vMinGamezone.x),                    Mathf.Min((m_vMaxPosition.y - vMinGamezone.y + vMaxGamezone.y) / 2, fVisionHeight - 5 - vMinGamezone.y));
    }
    public void SetMaxPosition(Vector2 _vMaxPosition)
    {
        m_vMaxPosition = _vMaxPosition;
        ComputeRealMaxPositions();
        ClampPosition();
    }
    public bool isInInterface(Vector2 _vPosition)
    {
        if (_vPosition.x > m_iLeftInterfaceWidth && _vPosition.x < Screen.width - m_iRightInterfaceWidth && _vPosition.y > m_iBottomInterfaceHeight && _vPosition.y < Screen.height - m_iTopInterfaceHeight)
        {
            return false;
        }
        return true;
    }
}
