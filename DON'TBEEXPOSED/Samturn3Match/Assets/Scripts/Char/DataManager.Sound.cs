using UnityEngine;
using System.Collections;

public partial class DataManager : MonoBehaviour{

	public AudioClip sound_eff01;
	public AudioClip sound_cut01;
	public AudioClip sound_swordswing1;

	public AudioClip bgm1;
	public AudioClip bgm2;
	public AudioSource playerAudio;

	public void SoundPlay(AudioClip sound, float volume){
		playerAudio.PlayOneShot (sound, volume);	
	}
	public void SoundPlayBGM(AudioClip sound){
		playerAudio.loop = true;
		playerAudio.clip = sound;
		playerAudio.Play();
	}
	public void SoundPlayBGMOff(){
		playerAudio.Stop();
	}
}
