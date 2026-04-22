using Stage;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UICheckoutTime : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_TextMeshPro;

    // Update is called once per frame
    void Update()
    {
        m_TextMeshPro.text = GetCheckoutTime().ToString();
        if (GetCheckoutTime().Equals(1)) m_TextMeshPro.color = Color.red;
        else m_TextMeshPro.color = Color.black;
    }

    private int GetCheckoutTime()
    {
        HumanData data = gameObject.GetComponent<HumanData>();
        return data.checkoutTime;
    }
}
