using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class next_level : MonoBehaviour
{
    public GameObject game_clear_canvas;
    public void ButtonClick() //��ư Ŭ�� �̺�Ʈ�� ���� �Լ��� ����� �ش�.
    {
        Cursor.lockState = CursorLockMode.Locked;
        game_clear_canvas.SetActive(false);
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
        Debug.Log("next_level");
    }
}
