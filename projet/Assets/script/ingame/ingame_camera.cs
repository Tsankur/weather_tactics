using UnityEngine;
using System.Collections;

public class ingame_camera : MonoBehaviour
{
    // setting interface size to set the correct game zone
    public int m_iTopInterfaceHeight = 0;
    public int m_iBottomInterfaceHeight = 0;
    public int m_iRightInterfaceWidth = 0;
    public int m_iLeftInterfaceWidth = 0;

    private Vector2 m_vMaxPosition = new Vector2(0, 0);
    private Vector2 m_vRealMaxPosition = new Vector2(0, 0);
    private Vector2 m_vRealMinPosition = new Vector2(0, 0);
    private bool m_bMoving = false;
    private Vector3 m_vLastMousePosition;
    //private float m_fZoom = 150;
    // Use this for initialization
    void Start()
    {
        // compute the zoom to have a minimum 10 cells on the smallest of width or height taking the UI into account.
        /*int iCenterWidth = Screen.width - (m_iRightInterfaceWidth + m_iLeftInterfaceWidth);
        int iCenterHeight = Screen.height - (m_iTopInterfaceHeight + m_iBottomInterfaceHeight);
        if(iCenterWidth > iCenterHeight)
        {
            m_fZoom = 100.0f * ((float)Screen.height / (float)iCenterHeight) / Mathf.Sin(Mathf.PI / 6.0f) * Mathf.Cos(Mathf.PI / 6.0f) / 2;
        }
        else
        {
            m_fZoom = 100.0f * ((float)Screen.width / (float)iCenterWidth) / Mathf.Sin(Mathf.PI / 6.0f) * Mathf.Cos(Mathf.PI / 6.0f) / camera.aspect / 2;
        }
        transform.position = new Vector3(transform.position.x, transform.position.y, -m_fZoom);*/
        int iCenterWidth = Screen.width - (m_iRightInterfaceWidth + m_iLeftInterfaceWidth);
        int iCenterHeight = Screen.height - (m_iTopInterfaceHeight + m_iBottomInterfaceHeight);
        if(iCenterWidth > iCenterHeight)
        {
            camera.orthographicSize = 100.0f * (float)Screen.height / (float)iCenterHeight / 2;
        }
        else
        {
            camera.orthographicSize = 100.0f * (float)Screen.width / (float)iCenterWidth / camera.aspect / 2;
        }
        transform.position = new Vector3(transform.position.x, transform.position.y, -50);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            m_vLastMousePosition = Input.mousePosition;
            m_bMoving = true;
        }
        if (m_bMoving)
        {
            //transform.position += (m_vLastMousePosition - Input.mousePosition) / (800.0f / m_fZoom);
            transform.position += (m_vLastMousePosition - Input.mousePosition) / 450.0f * camera.orthographicSize;
            ClampPosition();
            m_vLastMousePosition = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0))
        {
            m_bMoving = false;
        }
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
        //transform.position = new Vector3(newX, newY, -m_fZoom);
        transform.position = new Vector3(newX, newY, -50);
    }
    void ComputeRealMaxPositions()
    {
        /*float fVisionHeight = m_fZoom * Mathf.Sin(Mathf.PI / 6.0f) / Mathf.Cos(Mathf.PI / 6.0f);
        float fVisionWidth = fVisionHeight * camera.aspect;*/
        float fVisionHeight = camera.orthographicSize;
        float fVisionWidth = fVisionHeight * camera.aspect;
        Vector2 vMinGamezone = new Vector2(m_iLeftInterfaceWidth / (float)Screen.width * fVisionWidth * 2, m_iBottomInterfaceHeight / (float)Screen.height * fVisionHeight * 2);
        Vector2 vMaxGamezone = new Vector2(m_iRightInterfaceWidth / (float)Screen.width * fVisionWidth * 2, m_iTopInterfaceHeight / (float)Screen.height * fVisionHeight * 2);
        m_vRealMaxPosition = new Vector2(Mathf.Max((m_vMaxPosition.x - vMinGamezone.x + vMaxGamezone.x) / 2, m_vMaxPosition.x - fVisionWidth + 5 + vMaxGamezone.x), Mathf.Max((m_vMaxPosition.y - vMinGamezone.y + vMaxGamezone.y) / 2, m_vMaxPosition.y - fVisionHeight + 5 + vMaxGamezone.y));
        m_vRealMinPosition = new Vector2(Mathf.Min((m_vMaxPosition.x - vMinGamezone.x + vMaxGamezone.x) / 2, fVisionWidth - 5 - vMinGamezone.x), Mathf.Min((m_vMaxPosition.y - vMinGamezone.y + vMaxGamezone.y) / 2, fVisionHeight - 5 - vMinGamezone.y));

        //transform.position = new Vector3(transform.position.x, m_vRealMaxPosition.y, -m_fZoom);
        transform.position = new Vector3(transform.position.x, m_vRealMaxPosition.y, -50);
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
