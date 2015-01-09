using UnityEngine;
using System.Collections;

public class editor_camera : MonoBehaviour
{
    private Vector2 m_vMaxPosition = new Vector3(0, 0);
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
	    if(Input.GetMouseButtonDown(1))
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
        }
        if (m_bMoving)
        {
            transform.position += (m_vLastMousePosition - Input.mousePosition) / (800.0f / m_fZoom);
            float newX = transform.position.x, newY = transform.position.y;
            if (transform.position.x > m_vMaxPosition.x)
            {
                newX = m_vMaxPosition.x;
            }
            if (transform.position.y > m_vMaxPosition.y)
            {
                newY = m_vMaxPosition.y;
            }
            if (transform.position.x < 0)
            {
                newX = 0;
            }
            if (transform.position.y < 0)
            {
                newY = 0;
            }
            transform.position = new Vector3(newX, newY, -m_fZoom);
            m_vLastMousePosition = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(1))
        {
            m_bMoving = false;
        }
	}
    public void SetMaxPosition(Vector2 _vMaxPosition)
    {
        m_vMaxPosition = _vMaxPosition;
        if (transform.position.x > m_vMaxPosition.x)
        {
            transform.position = new Vector3(m_vMaxPosition.x, transform.position.y, -m_fZoom);
        }
        if (transform.position.y > m_vMaxPosition.y)
        {
            transform.position = new Vector3(transform.position.x, m_vMaxPosition.y, -m_fZoom);
        }
    }
}
