package com.cc.vibration;
import android.app.Activity;
import android.content.Context;
import android.os.Vibrator;
import android.util.Log;
public class Vibration {
	public static void Vibrate(Activity a)
	{		
		Vibrator v = (Vibrator) a.getSystemService(Context.VIBRATOR_SERVICE);	
		v.vibrate(100);	
	}
	
	public static void VibrateForSeconds(Activity a, long time)
	{		
		Vibrator v = (Vibrator) a.getSystemService(Context.VIBRATOR_SERVICE);	
		v.vibrate(time);	
	}
}
