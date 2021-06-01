using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    //Bateria de sonidos del objeto
    public Sound[] sounds;
    //Se trata del controlador de sonidos independientes de la ubicacion de la fuente (principal) ?
    public bool ubiquitous = false;
    //Referencia estática al main audio manager para mayor comodidad
    public static AudioManager mainManager;

    public AudioSource source;
    public float playDelay = 0.2f;
    private float currDelay;

    void Awake()
    {
        //Si el manager es ubicuo será el principal
        if(ubiquitous)
        {
            if (mainManager == null)
            {
                mainManager = this;
            }
            else
                return;    
        }

        source = GetComponent<AudioSource>();

        //Si no es sonido 3D (no cuenta con una source en el objeto), modificamos su fuente con las caracteristicas elegidas
        if (source == null)
        {
            foreach (Sound s in sounds)
            {
                s.source = gameObject.AddComponent<AudioSource>();
                s.source.clip = s.clip;
                s.source.volume = s.volume;
                s.source.pitch = s.pitch;
                s.source.loop = s.loop;
            }
        }
    }

    //Reproducir un sonido del array
    public void Play (string name)
    {
        Sound s = System.Array.Find(sounds, sound => sound.name == name);
        PlaySound(s);
    }

    //Está un sonido reproduciendose?
    public bool IsPlaying(string name)
    {
        Sound s = System.Array.Find(sounds, sound => sound.name == name);

        if (source == null)
            return s.source.isPlaying;
        else
            return source.isPlaying;
    }

    private void PlaySound(Sound sound)
    {
        if(source == null)
            sound.source.Play();
        else
        {
            if(currDelay > playDelay)
            {
                source.clip = sound.clip;
                source.Play();
                currDelay = 0;
            }
        }
    }

    private void Update()
    {
        //Evitar demasiados cambios de sonidos
        currDelay += Time.deltaTime;
    }

    //Parar un sonido del array
    public void Stop (string name)
    {
        Sound s = System.Array.Find(sounds, sound => sound.name == name);
        StopSound(s);
    }

    private void StopSound(Sound sound)
    {
        if (source == null)
            sound.source.Stop();
        else
            source.Stop();
    }

    //Reproduce un sonido aleatorio de la bateria
    public void RandomPlay()
    {
        int nSound = Random.Range(0, sounds.Length);
        PlaySound(sounds[nSound]);
    }
}
