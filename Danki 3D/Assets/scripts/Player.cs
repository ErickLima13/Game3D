using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    
    private CharacterController controller;

    [Header("Atributtes")]
    public float totalHealth;
    public float speed;
    public float gravity;
    public float damage = 20;

    private Animator anim;

    private Transform cam;

    Vector3 moveDirection;

    private bool isWalking;
    private bool waitFor;
    private bool isHitting;
    public bool isDead;


    public float smoothRotTime;
    private float turnSmoothVelocity;

    public float colliderRadius;
    public List<Transform> enemyList = new List<Transform>();


    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        cam = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isDead)
        {
            Move();
            GetMouseInput();
        }
        
        
    }

    //metodo da movimentação do personagem
    void Move()
    {
        if (controller.isGrounded)
        {
            //pega o input horizontal (teclas direita/esquerda)
            float horizontal = Input.GetAxisRaw("Horizontal");

            //pega o input vertical (teclas cima/baixo)
            float vertical = Input.GetAxisRaw("Vertical");

            //variavel local que armzazena os eixos horizontal e vertical
            Vector3 direction = new Vector3(horizontal, 0f, vertical);

            //verifica se o personagem está movimentando (caso o input seja maior que zero)
            if (direction.magnitude > 0)
            {
                if (!anim.GetBool("attacking"))
                {
                    //variavel local que armazena a rotação e o angulo de vizualização da camera 
                    float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;

                    //variavel local que armazena a rotacao porem mais suave
                    float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, angle, ref turnSmoothVelocity, smoothRotTime);

                    //passamos a rotação suave ao personagem
                    transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);

                    //armazena a direção com base na direção do mouse
                    moveDirection = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * speed;

                    //chama a animação walk
                    anim.SetInteger("transition", 1);

                    isWalking = true;
                }
                else
                {
                    anim.SetBool("walking", false);
                    moveDirection = Vector3.zero;
                }
            }
            //freia o personagem
            else if(isWalking)
            {
                // é executado quando o personagem está parado
                anim.SetBool("walking", false);

                //chama a animação iddle
                anim.SetInteger("transition", 0);
                moveDirection = Vector3.zero;

                isWalking = false;
            }
        }

        moveDirection.y -= gravity * Time.deltaTime;

        //move o personagem
        controller.Move(moveDirection  * Time.deltaTime);

    }

    void GetMouseInput()
    {
        if (controller.isGrounded)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (anim.GetBool("walking"))
                {
                    anim.SetBool("walking", false);
                    anim.SetInteger("transition", 0);
                }

                if (!anim.GetBool("walking"))
                {
                    StartCoroutine("Attack");
                }

            }
        }
    }

    IEnumerator Attack()
    {
        if (!waitFor && !isHitting)
        {
            waitFor = true;
            anim.SetBool("attacking", true);
            anim.SetInteger("transition", 2);

            yield return new WaitForSeconds(0.4f);

            GetEnemiesList();

            foreach (Transform e in enemyList)
            {
                //aplica dano ao inimigo
                CombatEnemy enemy = e.GetComponent<CombatEnemy>();

                if (enemy != null)
                {
                    enemy.GetHit(damage);
                    
                }
            }

            yield return new WaitForSeconds(1f);

            anim.SetInteger("transition", 0);
            anim.SetBool("attacking", false);
            waitFor = false;
        }
    }

    void GetEnemiesList()
    {
        enemyList.Clear();
        foreach (Collider c in Physics.OverlapSphere((transform.position + transform.forward * colliderRadius), colliderRadius))
        {
            if (c.gameObject.CompareTag("Enemy"))
            {
                enemyList.Add(c.transform);
            }
        }
    }

    public void GetHit(float damage)
    {
        totalHealth -= damage;

        if(totalHealth > 0)
        {
            //player ainda está vivo
            StopCoroutine("Attack");
            anim.SetInteger("transition", 3);
            isHitting = true;
            StartCoroutine("RecoveryFromHit");
        }
        else
        {
            //player morre
            isDead = true;
            anim.SetTrigger("die");
        }
    }

    IEnumerator RecoveryFromHit()
    {
        yield return new WaitForSeconds(1f);
        anim.SetInteger("transition", 0);
        isHitting = false;
        anim.SetBool("attacking", false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward, colliderRadius);
    }

}
