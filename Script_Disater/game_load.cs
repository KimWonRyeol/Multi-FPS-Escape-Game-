using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class game_load : MonoBehaviour
{
    public void ButtonClick() //��ư Ŭ�� �̺�Ʈ�� ���� �Լ��� ����� �ش�.
    {
        SceneManager.LoadScene("Scene_02");
    }
}
