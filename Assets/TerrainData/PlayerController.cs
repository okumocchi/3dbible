using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;  // CharacterController型の変数
    private Vector3 moveVelocity;  // キャラクターコントローラーを動かすためのVector3型の変数
    //[SerializeField] private Animator animator;
    [SerializeField] public Transform verRot;  //縦の視点移動の変数(カメラに合わせる)
    [SerializeField] public Transform horRot;  //横の視点移動の変数(プレイヤーに合わせる)
    [SerializeField] public float moveSpeed;  //移動速度
    [SerializeField] public float jumpPower;  //ジャンプ力

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        //moveVelocity = Vector3.zero;
        // マウスによる視点操作
        float X_Rotation = Input.GetAxis("Mouse X");
        float Y_Rotation = Input.GetAxis("Mouse Y");
        horRot.transform.Rotate(new Vector3(0, X_Rotation * 2, 0));
        verRot.transform.Rotate(-Y_Rotation * 2, 0, 0);

        //Wキーがおされたら
        if(Input.GetKey(KeyCode.W))
        {
            //characterController.Move(this.gameObject.transform.forward * moveSpeed * Time.deltaTime);
            characterController.Move(horRot.transform.forward * moveSpeed * Time.deltaTime);
        }
        //Sキーがおされたら
        if (Input.GetKey (KeyCode.S))
		{
			//characterController.Move (this.gameObject.transform.forward * -1f * moveSpeed * Time.deltaTime);
			characterController.Move (horRot.transform.forward * -1f * moveSpeed * Time.deltaTime);
		}
		//Aキーがおされたら
		if (Input.GetKey (KeyCode.A))
		{
			//characterController.Move (this.gameObject.transform.right * -1 * moveSpeed * Time.deltaTime);
			characterController.Move (horRot.transform.right * -1 * moveSpeed * Time.deltaTime);
		}
		//Dキーがおされたら
		if (Input.GetKey (KeyCode.D))
		{
			//characterController.Move (this.gameObject.transform.right * moveSpeed * Time.deltaTime);
			characterController.Move (horRot.transform.right * moveSpeed * Time.deltaTime);
		}

        // 接地しているとき
        if(characterController.isGrounded)
        {
            // ジャンプ
            if(Input.GetKeyDown(KeyCode.Space))
            {
                moveVelocity.y = jumpPower;
            }
        }
        // 空中にいる時
        else
        {
            // 重力をかける
            moveVelocity.y += Physics.gravity.y * Time.deltaTime;
        }

        // キャラクターを動かす
        characterController.Move(moveVelocity * Time.deltaTime);

        // 移動スピードをアニメーターに反映する
        //animator.SetFloat("MoveSpeed", new Vector3(moveVelocity.x, 0, moveVelocity.z).magnitude);
    }
}