using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class game_start : MonoBehaviour
{
    public GameObject game_clear_canvas;
    public GameObject level_obj;

    public void ButtonClick() //��ư Ŭ�� �̺�Ʈ�� ���� �Լ��� ����� �ش�.
    {

        GameObject level_obj = GameObject.Find("level_count");
        level_obj.GetComponent<level_manger>().level += 1;

        Cursor.lockState = CursorLockMode.Locked;
        game_clear_canvas.SetActive(false);
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }
}
