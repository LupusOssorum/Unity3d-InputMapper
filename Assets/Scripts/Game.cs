﻿using UnityEngine;
using System;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.IO;
using ws.winx.input;
using System.Runtime.Serialization;
using System.Linq;
using ws.winx.platform;
using ws.winx.devices;
using ws.winx.input.states;
using System.Collections;
using ws.winx.gui;
using ws.winx.unity;

namespace ws.winx
{
    public class Game : MonoBehaviour
    {

        // What if you have 50 states. Would you need to define them manually. => Forget it about this
        //		static int idleState = Animator.StringToHash("Base Layer.Idle");	
        //		static int locoState = Animator.StringToHash("Base Layer.Locomotion");			// these integers are references to our animator's states
        //		static int jumpState = Animator.StringToHash("Base Layer.Jump");				// and are used to check state for various actions to occur
        //		static int jumpDownState = Animator.StringToHash("Base Layer.JumpDown");		// within our FixedUpdate() function below
        //		static int fallState = Animator.StringToHash("Base Layer.Fall");
        //		static int rollState = Animator.StringToHash("Base Layer.Roll");
        //		static int waveState = Animator.StringToHash("Layer2.Wave");



        Animator animator=null;
		bool _settingsLoaded=false;


        // Use this for initialization
        void Start()
        {
            animator = GameObject.FindObjectOfType<Animator>();

            


            //supporting devices with custom drivers
#if UNITY_STANDALONE_WIN
			    InputManager.AddDriver(new ws.winx.platform.windows.XInputDriver());
#endif



            //if you want to load some states from .xml and add custom manually first load settings xml
            //!!!Application.streamingAssetPath gives "Raw" folder in web player
        
#if (UNITY_STANDALONE || UNITY_EDITOR) && !UNITY_WEBPLAYER
			UnityEngine.Debug.Log("Standalone");
                  
			UserInterfaceWindow ui=this.GetComponent<UserInterfaceWindow>();
			if(ui!=null && ui.settingsXML==null){
                InputManager.loadSettings(Path.Combine(Path.Combine(Application.dataPath, "StreamingAssets"), "InputSettings.xml"));

				ui.StateInputCombinations=InputManager.Settings.stateInputs;
			}


			//		adding input-states pairs manually
			//			InputManager.MapStateToInput("My State1",new InputCombination(KeyCodeExtension.toCode(Joysticks.Joystick1,JoystickAxis.AxisPovX,JoystickPovPosition.Forward),(int)KeyCode.Joystick4Button9,(int)KeyCode.P,(int)KeyCode.JoystickButton0));
			//			InputManager.MapStateToInput("My State2",new InputCombination(KeyCode.Joystick4Button9,KeyCode.P,KeyCode.JoystickButton0));
			//			InputManager.MapStateToInput("My State3",new InputCombination("A(x2)+Mouse1+JoystickButton31"));
			//			InputManager.MapStateToInput("My State1",new InputCombination("Mouse1+Joystick12AxisXPositive(x2)+B"));
			
			
			//easiest way to map state to combination (ex.of single W and C click)
            InputManager.MapStateToInput("ManualAddedSTATE", KeyCodeExtension.W.SINGLE, KeyCodeExtension.C.SINGLE);
			
			UnityEngine.Debug.Log("Log:" + InputManager.Log());
			
			
			////Event Based input handling
            InputEvent ev = new InputEvent("ManualAddedSTATE");
			//InputEvent ev = new InputEvent((int)States.SomeState);
			
			ev.INPUT += new EventHandler(Handle1);
			ev.INPUT += new EventHandler(Handle2);
			ev.UP += new EventHandler(onUp);//this wouldn't fire for combo inputs(single only)
			ev.DOWN += new EventHandler(onDown);//this wouldn't fire for combo inputs(single only)



			_settingsLoaded=true;
#endif


#if(UNITY_WEBPLAYER || UNITY_EDITOR) && !UNITY_STANDALONE
            Loader request = new Loader();
            request.Add(Application.dataPath+"/StreamingAssets/InputSettings.xml");
            request.LoadComplete += new EventHandler<LoaderEvtArgs>(onLoadComplete);
            request.Error += new EventHandler<LoaderEvtArgs>(onLoadItemComplete);
            request.LoadItemComplete += new EventHandler<LoaderEvtArgs>(onLoadItemComplete);
            request.load();
#endif












        }


        void onUp(object o, EventArgs args)
        {
            Debug.Log("Up");
        }

        void onDown(object o, EventArgs args)
        {
            Debug.Log("Down");
        }

        void Handle1(object o, EventArgs args)
        {
            Debug.Log("Handle1");
        }

        void Handle2(object o, EventArgs args)
        {
            Debug.Log("Handle2");
        }

		#if (UNITY_WEBPLAYER || UNITY_EDITOR) && !UNITY_STANDALONE
        void onLoadComplete(object sender, LoaderEvtArgs args)
        {
           // Debug.Log(((List<WWW>)args.data).ElementAt(0).text);


			UnityEngine.Debug.Log("WebPlayer " + Path.Combine(Path.Combine(Application.dataPath, "StreamingAssets"), "InputSettings.xml"));
			
			

			UserInterfaceWindow ui=this.GetComponent<UserInterfaceWindow>();


            if (ui != null)//without settingsXML defined =>load them manually and attach them
            {
                InputManager.loadSettingsFromText(((List<WWW>)args.data).ElementAt(0).text);
                ui.StateInputCombinations = InputManager.Settings.stateInputs;
            }
          
			//   UnityEngine.Debug.Log(InputManager.Log());
			
			//		adding input-states pairs manually
			//			InputManager.MapStateToInput("My State1",new InputCombination(KeyCodeExtension.toCode(Joysticks.Joystick1,JoystickAxis.AxisPovX,JoystickPovPosition.Forward),(int)KeyCode.Joystick4Button9,(int)KeyCode.P,(int)KeyCode.JoystickButton0));
			//			InputManager.MapStateToInput("My State2",new InputCombination(KeyCode.Joystick4Button9,KeyCode.P,KeyCode.JoystickButton0));
			//			InputManager.MapStateToInput("My State3",new InputCombination("A(x2)+Mouse1+JoystickButton31"));
			//			InputManager.MapStateToInput("My State1",new InputCombination("Mouse1+Joystick12AxisXPositive(x2)+B"));
			
			
			//easiest way to map state to combination (ex.of single W and C click)
			InputManager.MapStateToInput("ManualAddedSTATE", KeyCodeExtension.W.SINGLE, KeyCodeExtension.C.SINGLE);
			
			UnityEngine.Debug.Log("Log:" + InputManager.Log());
			
			
			////Event Based input handling
			InputEvent ev = new InputEvent("ManualAddedSTATE");
			//InputEvent ev = new InputEvent((int)States.SomeState);
			
			ev.INPUT += new EventHandler(Handle1);
			ev.INPUT += new EventHandler(Handle2);
			ev.UP += new EventHandler(onUp);//this wouldn't fire for combo inputs(single only)
			ev.DOWN += new EventHandler(onDown);//this wouldn't fire for combo inputs(single only)

            _settingsLoaded = true;
        }

        void onLoadItemComplete(object sender, LoaderEvtArgs args)
        {
           // Debug.Log(((WWW)args.data).text);
        }


        void onLoadError(object sender, LoaderEvtArgs args)
        {
             Debug.Log(((WWW)args.data).error);
        }
#endif



        // Update is called once per frame
        void Update()
        {

			//Use is mapping states so no quering keys during gameplay
			if (InputManager.EditMode ||  !_settingsLoaded) return ;


            //Input.GetInput allows combos (combined input actions)
            if (InputManager.GetInputDown((int)States.Wave))// || InputManager.GetInput((int)States.Wave,true))
            {
                Debug.Log("Wave Down");
                animator.Play((int)States.Wave);
            }


            if (InputManager.GetInputUp((int)States.MyCustomState))
            {
                Debug.Log(States.MyCustomState + "-Up");
                // animator.Play((int)States.Wave);
            }


           

            //if (InputManager.GetInput((int)States.MyCustomState, false))
            //{
            //    Debug.Log(States.MyCustomState + "-Hold");
            //    // animator.Play((int)States.Wave);
            //}


            //if (InputManager.GetInputUp((int)States.Wave))
            //{
            //    Debug.Log(States.Wave + "-Up");
            //    // animator.Play((int)States.Wave);
            //}



            //        if (InputManager.GetInput((int)States.Walk_Forward, false))
            //        {
            //
            //        }
            //	
            //
            //          if(InputManager.GetInputDown((int)States.Walk_Forward)){
            //				Debug.Log("Down");
            //			}
            //
            //			if(InputManager.GetInputUp((int)States.Walk_Forward)){
            //				Debug.Log("Up");
            //			}
            //
            ////		
            ////
            //          //using input as analog value
            //			float analogValue=InputManager.GetInput((int)States.Walk_Forward,false,0.3f,0.1f,0f);
            //			analogValue-=InputManager.GetInput((int)States.Base_Layer_MyState,false,0.3f,0.1f,0f);
            //
            ////			Debug.Log(analogValue);





        }





        /// <summary>
        /// DONT FORGET TO CLEAN AFTER YOURSELF
        /// </summary>
        void OnDestroy()
        {
            InputManager.Dispose();
        }
    }
}
