using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

enum TaxiStatus
{
    Idel,
    Accerate,
    Start
}

public class Taxi : MonoBehaviour
{
    public GameObject explosion;

    [HideInInspector]
    public int damage = 0;
    private int maxDamage = 50;

    private Animator animator;

    [HideInInspector]
    public bool IsForhire = true;
    public bool IsOpendoor = false;

    private Rigidbody2D rb;

    private float horizontalInput = 0;
    private float verticalInput = 0;

    private int movementSpeed = 2;
    private int rotationSpeed = 2;

    private float driftFactorSticky = 0.9f;
    private float driftFactorSlippy = 1;
    private float maxStickyVelocity = 2.5f;

    public AudioClip[] SFXs;
    private AudioSource carAudioSource;
    private AudioSource explosionAudioSource;
    private AudioSource hintAudioSource;

    private TaxiStatus currentStatus;
    private TaxiStatus previousStatus;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        var audioSources = GetComponentsInChildren<AudioSource>();
        carAudioSource = audioSources[0];
        explosionAudioSource = audioSources[1];
        hintAudioSource = audioSources[2];
        currentStatus = TaxiStatus.Idel;
        previousStatus = TaxiStatus.Start;
        StartCoroutine(CarSoundsPlaying());

#if UNITY_ANDROID

        InputSystem.EnableDevice(UnityEngine.InputSystem.Gyroscope.current);
        rotationSpeed = 3;

#endif
    }

    private void FixedUpdate()
    {
        if (IsOpendoor) verticalInput = 0;

        animator.SetFloat("Speed", rb.velocity.magnitude);
        animator.SetBool("IsForhire", IsForhire);

        float driftFactor = driftFactorSticky;
        if (RightVelocity().magnitude > maxStickyVelocity)
            driftFactor = driftFactorSlippy;

        rb.velocity = ForwardVelocity() + RightVelocity() * driftFactor;

#if !UNITY_ANDROID

        DealwithVerticalInput();
        DealwithHorizontalInput();

#endif

        // Debug.Log("Speed= " + rb.velocity.magnitude);
        // Debug.Log("Status= " + currentStatus);
        // Debug.Log("p Status= " + previousStatus);

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ++damage;
        Instantiate(explosion, transform.position, Quaternion.identity);
        explosionAudioSource.Play();
        if (damage > maxDamage)
        {
            SceneManager.LoadScene("TakeYouHomeOverScene");
        }
    }

    private void DealwithVerticalInput()
    {
        if (verticalInput > 0)
        {
            rb.AddForce(transform.up * movementSpeed);
            previousStatus = currentStatus;
            currentStatus = TaxiStatus.Accerate;
            return;
        }

        if (verticalInput == 0) rb.AddForce(-rb.velocity);
        else rb.AddForce(-transform.up * movementSpeed);

        previousStatus = currentStatus;
        currentStatus = TaxiStatus.Idel;
    }

    private void DealwithHorizontalInput()
    {
        rb.rotation += (-horizontalInput) * rotationSpeed;
    }

    Vector2 ForwardVelocity()
    {
        return transform.up * Vector2.Dot(GetComponent<Rigidbody2D>().velocity, transform.up);
    }

    Vector2 RightVelocity()
    {
        return transform.right * Vector2.Dot(GetComponent<Rigidbody2D>().velocity, transform.right);
    }

#if UNITY_ANDROID

    private bool notValid = false;
    private float rotationTiggerValue = 1.8f;

    private IEnumerator InvalidHorizontalInput()
    {
        notValid = true;
        yield return new WaitForSeconds(1);
        notValid = false;
    }
    private void OnTurn(InputValue inputValue)
    {
        if (IsOpendoor || notValid) return;
        horizontalInput = inputValue.Get<float>();
        DealwithHorizontalInput();
        if (Mathf.Abs(horizontalInput) > rotationTiggerValue)
        {
            //Debug.Log(horizontalInput.ToString());
            StartCoroutine(InvalidHorizontalInput());
        }
    }

    private void OnGas(InputValue inputValue)
    {
        if (IsOpendoor) return;
        verticalInput = inputValue.Get<float>();
        DealwithVerticalInput();
    }

#else

    private void OnMove(InputValue inputValue)
    {
        if (IsOpendoor) return;
        horizontalInput = inputValue.Get<Vector2>().x;
        verticalInput = inputValue.Get<Vector2>().y;
    }

#endif
    private void OnOpenDoor()
    {
        IsOpendoor = !IsOpendoor;
        animator.SetBool("IsOpendoor", IsOpendoor);
    }

    public void PlayStartingHintSound()
    {
        hintAudioSource.clip = SFXs[0];
        hintAudioSource.Play();
    }

    public void PlayTerminalHintSound()
    {
        hintAudioSource.clip = SFXs[1];
        hintAudioSource.Play();
    }

    private IEnumerator CarSoundsPlaying()
    {
        while (true)
        {
            // If status changed, car audio must stop.
            if (currentStatus != previousStatus)
            {
                carAudioSource.Stop();
                carAudioSource.loop = false;
            }

            switch (currentStatus)
            {
                case TaxiStatus.Accerate:
                    if (previousStatus == TaxiStatus.Accerate)
                    {
                        if (carAudioSource.isPlaying == false)
                        {
                            carAudioSource.clip = SFXs[4];
                            carAudioSource.loop = true;
                            carAudioSource.Play();
                        }
                    }
                    else
                    {
                        carAudioSource.clip = SFXs[3];
                        carAudioSource.loop = false;
                        carAudioSource.volume = 0.4f;
                        carAudioSource.Play();
                    }
                    break;
                case TaxiStatus.Idel:
                    if (previousStatus == TaxiStatus.Idel) break;
                    carAudioSource.clip = SFXs[2];
                    carAudioSource.loop = true;
                    carAudioSource.volume = 0.2f;
                    carAudioSource.Play();
                    break;
            }
            yield return null;
        }
    }
}
