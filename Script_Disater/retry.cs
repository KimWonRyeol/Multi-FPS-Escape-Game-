using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class retry : MonoBehaviour
{
    public GameObject game_clear_canvas;
    public GameObject player;
 
    public void ButtonClick() //��ư Ŭ�� �̺�Ʈ�� ���� �Լ��� ����� �ش�.
    {
        Cursor.lockState = CursorLockMode.Locked;
        game_clear_canvas.SetActive(false);
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }
}
