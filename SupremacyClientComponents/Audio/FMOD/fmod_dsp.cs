
/* ========================================================================================== */
/* FMOD Ex - DSP header file. Copyright (c), Firelight Technologies Pty, Ltd. 2004-2005.      */
/*                                                                                            */
/* Use this header if you are interested in delving deeper into the FMOD software mixing /    */
/* DSP engine.  In this header you can find parameter structures for FMOD system reigstered   */
/* DSP effects and generators.                                                                */
/*                                                                                            */
/* ========================================================================================== */

using System;
using System.Text;
using System.Runtime.InteropServices;

namespace FMOD
{
    /* 
        DSP callbacks
    */
    public delegate RESULT DSP_CREATECALLBACK         (ref DSP_STATE dsp_state);
    public delegate RESULT DSP_RELEASECALLBACK        (ref DSP_STATE dsp_state);
    public delegate RESULT DSP_RESETCALLBACK          (ref DSP_STATE dsp_state);
    public delegate RESULT DSP_READCALLBACK           (ref DSP_STATE dsp_state, IntPtr inbuffer, IntPtr outbuffer, uint length, int inchannels, int outchannels);
    public delegate RESULT DSP_SETPOSITIONCALLBACK    (ref DSP_STATE dsp_state, uint seeklen);
    public delegate RESULT DSP_SETPARAMCALLBACK       (ref DSP_STATE dsp_state, int index, float val);
    public delegate RESULT DSP_GETPARAMCALLBACK       (ref DSP_STATE dsp_state, int index, ref float val, StringBuilder valuestr);
    public delegate RESULT DSP_DIALOGCALLBACK         (ref DSP_STATE dsp_state, IntPtr hwnd, bool show);


    /*
    [ENUM]
    [
        [DESCRIPTION]   
        These definitions can be used for creating FMOD defined special effects or DSP units.

        [REMARKS]
        To get them to be active, first create the unit, then add it somewhere into the DSP network, either at the front of the network near the soundcard unit to affect the global output (by using System::getDSPHead), or on a single channel (using Channel::getDSPHead).

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, XBox, PlayStation 2, GameCube

        [SEE_ALSO]
        System::createDSPByType
    ]
    */
    public enum DSP_TYPE
    {
        UNKNOWN,            /* This unit was created via a non FMOD plugin so has an unknown purpose */
        MIXER,              /* This unit does nothing but take inputs and mix them together then feed the result to the soundcard unit. */
        OSCILLATOR,         /* This unit generates sine/square/saw/triangle or noise tones. */
        LOWPASS,            /* This unit filters data using a resonant lowpass filter algorithm. */
        ITLOWPASS,          /* This unit filters sound using a resonant lowpass filter algorithm that is used in Impulse Tracker. */
        HIGHPASS,           /* This unit filters sound using a resonant highpass filter algorithm. */
        ECHO,               /* This unit produces an echo on the sound and fades out at the desired rate. */
        FLANGE,             /* This unit produces a flange effect on the sound. */
        DISTORTION,         /* This unit distorts the sound. */
        NORMALIZE,          /* This unit normalizes or amplifies the sound to a certain level. */
        PARAMEQ,            /* This unit attenuates or amplifies a selected frequency range. */
        PITCHSHIFT,         /* This unit bends the pitch of a sound without changing the speed of playback. */
        CHORUS,             /* This unit produces a chorus effect on the sound. */
        REVERB,             /* This unit produces a reverb effect on the sound. */
        VSTPLUGIN,          /* This unit allows the use of Steinberg VST plugins */
        WINAMPPLUGIN,       /* This unit allows the use of Nullsoft Winamp plugins */
        ITECHO,             /* This unit produces an echo on the sound and fades out at the desired rate as is used in Impulse Tracker. */
        COMPRESSOR          /* This unit implements dynamic compression (linked multichannel, wideband) */
    }


    /*
    [STRUCTURE]
    [
        [DESCRIPTION]   

        [REMARKS]
        Members marked with [in] mean the user sets the value before passing it to the function.<br>
        Members marked with [out] mean FMOD sets the value to be used after the function exits.<br>
        <br>
        The step parameter tells the gui or application that the parameter has a certain granularity.<br>
        For example in the example of cutoff frequency with a range from 100.0 to 22050.0 you might only want the selection to be in 10hz increments.  For this you would simply use 10.0 as the step value.<br>
        For a boolean, you can use min = 0.0, max = 1.0, step = 1.0.  This way the only possible values are 0.0 and 1.0.<br>
        Some applications may detect min = 0.0, max = 1.0, step = 1.0 and replace a graphical slider bar with a checkbox instead.<br>
        A step value of 1.0 would simulate integer values only.<br>
        A step value of 0.0 would mean the full floating point range is accessable.<br>

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, XBox, PlayStation 2, GameCube

        [SEE_ALSO]    
        System::createDSP
        System::getDSP
    ]
    */
    public struct DSP_PARAMETERDESC
    {
        public float         min;             /* [in] Minimum value of the parameter (ie 100.0). */
        public float         max;             /* [in] Maximum value of the parameter (ie 22050.0). */
        public float         defaultval;      /* [in] Default value of parameter. */
        public string        name;            /* [in] Name of the parameter to be displayed (ie "Cutoff frequency"). */
        public string        label;           /* [in] Short string to be put next to value to denote the unit design (ie "hz"). */
        public string        description;     /* [in] Description of the parameter to be displayed as a help item / tooltip for this parameter. */
    }


    /*
    [STRUCTURE] 
    [
        [DESCRIPTION]
        Strcture to define the parameters for a DSP unit.

        [REMARKS]
        Members marked with [in] mean the user sets the value before passing it to the function.<br>
        Members marked with [out] mean FMOD sets the value to be used after the function exits.<br>
        <br>
        There are 2 different ways to change a parameter in this architecture.<br>
        One is to use DSP::setParameter / DSP::getParameter.  This is platform independant and is dynamic, so new unknown plugins can have their parameters enumerated and used.<br>
        The other is to use DSP::showConfigDialog.  This is platform specific and requires a GUI, and will display a dialog box to configure the plugin.<br>
        
        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, XBox, PlayStation 2, GameCube

        [SEE_ALSO]
        System::createDSP
        System::getDSP
    ]
    */
    public struct DSP_DESCRIPTION
    {
        [MarshalAs(UnmanagedType.ByValArray,SizeConst=32)]
        public char[]                      name;               /* [in] Name of the unit to be displayed in the network. */
        public uint                        version;            /* [in] Plugin writer's version number. */
        public int                         channels;           /* [in] Number of channels.  Use 0 to process whatever number of channels is currently in the network.  >0 would be mostly used if the unit is a unit that only generates sound. */
        public DSP_CREATECALLBACK          create;             /* [in] Create callback.  This is called when DSP unit is created.  Can be null. */
        public DSP_RELEASECALLBACK         release;            /* [in] Release callback.  This is called just before the unit is freed so the user can do any cleanup needed for the unit.  Can be null. */
        public DSP_RESETCALLBACK           reset;              /* [in] Reset callback.  This is called by the user to reset any history buffers that may need resetting for a filter, when it is to be used or re-used for the first time to its initial clean state.  Use to avoid clicks or artifacts. */
        public DSP_READCALLBACK            read;               /* [in] Read callback.  Processing is done here.  Can be null. */
        public DSP_SETPOSITIONCALLBACK     setposition;        /* [in] Setposition callback.  This is called if the unit wants to update its position info but not process data.  Can be null. */

        public int                         numparameters;      /* [in] Number of parameters used in this filter.  The user finds this with DSP::getNumParameters */
        public DSP_PARAMETERDESC[]         paramdesc;          /* [in] Variable number of parameter structures. */
        public DSP_SETPARAMCALLBACK        setparameter;       /* [in] This is called when the user calls DSP::setParameter.  Can be null. */
        public DSP_GETPARAMCALLBACK        getparameter;       /* [in] This is called when the user calls DSP::getParameter.  Can be null. */
        public DSP_DIALOGCALLBACK          config;             /* [in] This is called when the user calls DSP::showConfigDialog.  Can be used to display a dialog to configure the filter.  Can be null. */
        public int                         configwidth;        /* [in] Width of config dialog graphic if there is one.  0 otherwise.*/
        public int                         configheight;       /* [in] Height of config dialog graphic if there is one.  0 otherwise.*/
        public IntPtr                      userdata;           /* [in] Optional. Specify 0 to ignore. This is user data to be attached to the DSP unit during creation.  Access via DSP::getUserData. */
    }


    /*
    [STRUCTURE] 
    [
        [DESCRIPTION]
        DSP plugin structure that is passed into each callback.

        [REMARKS]
        Members marked with [in] mean the variable can be written to.  The user can set the value.<br>
        Members marked with [out] mean the variable is modified by FMOD and is for reading purposes only.  Do not change this value.<br>

        [PLATFORMS]
        Win32, Win64, Linux, Linux64, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable, PlayStation 3

        [SEE_ALSO]
        FMOD_DSP_DESCRIPTION
    ]
    */
    public struct DSP_STATE
    {
        public IntPtr   instance;      /* [out] Handle to the DSP hand the user created.  Not to be modified.  C++ users cast to FMOD::DSP to use.  */
        public IntPtr   plugindata;    /* [in] Plugin writer created data the output author wants to attach to this object. */
    };


    /*
        ==============================================================================================================

        FMOD built in effect parameters.  
        Use DSP::setParameter with these enums for the 'index' parameter.

        ==============================================================================================================
    */

    /*
    [ENUM]
    [  
        [DESCRIPTION]   
        Parameter types for the FMOD_DSP_TYPE_OSCILLATOR filter.

        [REMARKS]

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, XBox, PlayStation 2, GameCube

        [SEE_ALSO]
        DSP::setParameter
        DSP::getParameter
        FMOD_DSP_TYPE   
    ]
    */
    public enum DSP_OSCILLATOR
    {
        TYPE,   /* Waveform design.  0 = sine.  1 = square. 2 = sawup. 3 = sawdown. 4 = triangle. 5 = noise.  */
        RATE    /* Frequency of the sinewave in hz.  1.0 to 22000.0.  Default = 220.0. */         
    }


    /*
    [ENUM]
    [  
        [DESCRIPTION]   
        Parameter types for the FMOD_DSP_TYPE_LOWPASS filter.

        [REMARKS]

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, XBox, PlayStation 2, GameCube

        [SEE_ALSO]      
        DSP::setParameter
        DSP::getParameter
        FMOD_DSP_TYPE
    ]
    */
    public enum DSP_LOWPASS
    {
        CUTOFF,    /* Lowpass cutoff frequency in hz.   1.0 to 22000.0.  Default = 5000.0. */
        RESONANCE  /* Lowpass resonance Q value. 1.0 to 10.0.  Default = 1.0. */
    }


    /*
    [ENUM]
    [  
        [DESCRIPTION]   
        Parameter types for the FMOD_DSP_TYPE_ITLOWPASS filter.

        [REMARKS]

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, XBox, PlayStation 2, GameCube

        [SEE_ALSO]      
        DSP::setParameter
        DSP::getParameter
        FMOD_DSP_TYPE
    ]
    */
    public enum DSP_ITLOWPASS
    {
        CUTOFF,    /* Lowpass cutoff frequency in hz.  1.0 to 22000.0.  Default = 5000.0/ */
        RESONANCE  /* Lowpass resonance Q value.  0.0 to 127.0.  Default = 1.0. */
    }


    /*
    [ENUM]
    [  
        [DESCRIPTION]   
        Parameter types for the FMOD_DSP_TYPE_HIGHPASS filter.

        [REMARKS]

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, XBox, PlayStation 2, GameCube

        [SEE_ALSO]      
        DSP::setParameter
        DSP::getParameter
        FMOD_DSP_TYPE
    ]
    */
    public enum DSP_HIGHPASS
    {
        CUTOFF,    /* Highpass cutoff frequency in hz.  10.0 to output 22000.0.  Default = 5000.0. */
        RESONANCE  /* Highpass resonance Q value.  1.0 to 10.0.  Default = 1.0. */
    }


    /*
    [ENUM]
    [  
        [DESCRIPTION]   
        Parameter types for the FMOD_DSP_TYPE_ECHO filter.

        [REMARKS]
        Note.  Every time the delay is changed, the plugin re-allocates the echo buffer.  This means the echo will dissapear at that time while it refills its new buffer.<br>
        Larger echo delays result in larger amounts of memory allocated.<br>
        <br>
        '<i>maxchannels</i>' also dictates the amount of memory allocated.  By default, the maxchannels value is 0.  If FMOD is set to stereo, the echo unit will allocate enough memory for 2 channels.  If it is 5.1, it will allocate enough memory for a 6 channel echo, etc.<br>
        If the echo effect is only ever applied to the global mix (ie it was added with System::addDSP), then 0 is the value to set as it will be enough to handle all speaker modes.<br>
        When the echo is added to a channel (ie Channel::addDSP) then the channel count that comes in could be anything from 1 to 8 possibly.  It is only in this case where you might want to increase the channel count above the output's channel count.<br>
        If a channel echo is set to a lower number than the sound's channel count that is coming in, it will not echo the sound.<br>

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, XBox, PlayStation 2, GameCube

        [SEE_ALSO]      
        DSP::setParameter
        DSP::getParameter
        FMOD_DSP_TYPE
    ]
    */
    public enum DSP_ECHO
    {
        DELAY,       /* Echo delay in ms.  10  to 5000.  Default = 500. */
        DECAYRATIO,  /* Echo decay per delay.  0 to 1.  1.0 = No decay, 0.0 = total decay.  Default = 0.5. */
        MAXCHANNELS, /* Maximum channels supported.  0 to 16.  0 = same as fmod's default output polyphony, 1 = mono, 2 = stereo etc.  See remarks for more.  Default = 0.  It is suggested to leave at 0! */
        DRYMIX,      /* Volume of original signal to pass to output.  0.0 to 1.0. Default = 1.0. */
        WETMIX       /* Volume of echo signal to pass to output.  0.0 to 1.0. Default = 1.0. */
    }


    /*
    [ENUM]
    [  
        [DESCRIPTION]   
        Parameter types for the FMOD_DSP_TYPE_FLANGE filter.

        [REMARKS]
        Flange is an effect where the signal is played twice at the same time, and one copy slides back and forth creating a whooshing or flanging effect.<br>
        As there are 2 copies of the same signal, by default each signal is given 50% mix, so that the total is not louder than the original unaffected signal.<br>
        <br>
        Flange depth is a percentage of a 10ms shift from the original signal.  Anything above 10ms is not considered flange because to the ear it begins to 'echo' so 10ms is the highest value possible.<br>

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, XBox, PlayStation 2, GameCube

        [SEE_ALSO]      
        DSP::setParameter
        DSP::getParameter
        FMOD_DSP_TYPE
    ]
    */
    public enum DSP_FLANGE
    {
        DRYMIX,      /* Volume of original signal to pass to output.  0.0 to 1.0. Default = 0.45. */
        WETMIX,      /* Volume of flange signal to pass to output.  0.0 to 1.0. Default = 0.55. */
        DEPTH,       /* Flange depth.  0.01 to 1.0.  Default = 1.0. */
        RATE         /* Flange speed in hz.  0.0 to 20.0.  Default = 0.1. */
    }


    /*
    [ENUM]
    [  
        [DESCRIPTION]   
        Parameter types for the FMOD_DSP_TYPE_DISTORTION filter.

        [REMARKS]

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, XBox, PlayStation 2, GameCube

        [SEE_ALSO]      
        DSP::setParameter
        DSP::getParameter
        FMOD_DSP_TYPE
    ]
    */
    public enum DSP_DISTORTION
    {
        LEVEL    /* Distortion value.  0.0 to 1.0.  Default = 0.5. */
    }


    /*
    [ENUM]
    [  
        [DESCRIPTION]   
        Parameter types for the FMOD_DSP_TYPE_NORMALIZE filter.

        [REMARKS]
        Normalize amplifies the sound based on the maximum peaks within the signal.<br>
        For example if the maximum peaks in the signal were 50% of the bandwidth, it would scale the whole sound by 2.<br>
        The lower threshold value makes the normalizer ignores peaks below a certain point, to avoid over-amplification if a loud signal suddenly came in, and also to avoid amplifying to maximum things like background hiss.<br>
        <br>
        Because FMOD is a realtime audio processor, it doesn't have the luxury of knowing the peak for the whole sound (ie it can't see into the future), so it has to process data as it comes in.<br>
        To avoid very sudden changes in volume level based on small samples of new data, fmod fades towards the desired amplification which makes for smooth gain control.  The fadetime parameter can control this.<br>

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, XBox, PlayStation 2, GameCube

        [SEE_ALSO]      
        DSP::setParameter
        DSP::getParameter
        FMOD_DSP_TYPE
    ]
    */
    public enum DSP_NORMALIZE
    {
        FADETIME,    /* Time to ramp the silence to full in ms.  0.0 to 20000.0. Default = 5000.0. */
        THRESHHOLD,  /* Lower volume range threshold to ignore.  0.0 to 1.0.  Default = 0.1.  Raise higher to stop amplification of very quiet signals. */
        MAXAMP       /* Maximum amplification allowed.  1.0 to 100000.0.  Default = 20.0.  1.0 = no amplifaction, higher values allow more boost. */
    }


    /*
    [ENUM]
    [  
        [DESCRIPTION]   
        Parameter types for the FMOD_DSP_TYPE_PARAMEQ filter.

        [REMARKS]
        Parametric EQ is a bandpass filter that attenuates or amplifies a selected frequency and its neighbouring frequencies.<br>
        <br>
        To create a multi-band EQ create multiple FMOD_DSP_TYPE_PARAMEQ units and set each unit to different frequencies, for example 1000hz, 2000hz, 4000hz, 8000hz, 16000hz with a range of 1 octave each.<br>
        <br>
        When a frequency has its gain set to 1.0, the sound will be unaffected and represents the original signal exactly.<br>

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, XBox, PlayStation 2, GameCube

        [SEE_ALSO]      
        DSP::setParameter
        DSP::getParameter
        FMOD_DSP_TYPE
    ]
    */
    public enum DSP_PARAMEQ
    {
        CENTER,     /* Frequency center.  20.0 to 22000.0.  Default = 8000.0. */
        BANDWIDTH,  /* Octave range around the center frequency to filter.  0.2 to 5.0.  Default = 1.0. */
        GAIN        /* Frequency Gain.  0.05 to 3.0.  Default = 1.0.  */
    }



    /*
    [ENUM]
    [  
        [DESCRIPTION]   
        Parameter types for the FMOD_DSP_TYPE_PITCHSHIFT filter.

        [REMARKS]
        This pitch shifting unit can be used to change the pitch of a sound without speeding it up or slowing it down.<br>
        It can also be used for time stretching or scaling, for example if the pitch was doubled, and the frequency of the sound was halved, the pitch of the sound would sound correct but it would be twice as slow.<br>
        <br>
        <b>Warning!</b> This filter is very computationally expensive!  Similar to a vocoder, it requires several overlapping FFT and IFFT's to produce smooth output, and can require around 440mhz for 1 stereo 48khz signal using the default settings.<br>
        Reducing the signal to mono will half the cpu usage, as will the overlap count.<br>
        Reducing this will lower audio quality, but what settings to use are largely dependant on the sound being played.  A noisy polyphonic signal will need higher overlap and fft size compared to a speaking voice for example.<br>
        <br>
        This pitch shifter is based on the pitch shifter code at http://www.dspdimension.com, written by Stephan M. Bernsee.<br>
        The original code is COPYRIGHT 1999-2003 Stephan M. Bernsee <smb@dspdimension.com>.<br>
        <br>
        '<i>maxchannels</i>' dictates the amount of memory allocated.  By default, the maxchannels value is 0.  If FMOD is set to stereo, the pitch shift unit will allocate enough memory for 2 channels.  If it is 5.1, it will allocate enough memory for a 6 channel pitch shift, etc.<br>
        If the pitch shift effect is only ever applied to the global mix (ie it was added with System::addDSP), then 0 is the value to set as it will be enough to handle all speaker modes.<br>
        When the pitch shift is added to a channel (ie Channel::addDSP) then the channel count that comes in could be anything from 1 to 8 possibly.  It is only in this case where you might want to increase the channel count above the output's channel count.<br>
        If a channel pitch shift is set to a lower number than the sound's channel count that is coming in, it will not pitch shift the sound.<br>

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, XBox, PlayStation 2, GameCube

        [SEE_ALSO]      
        DSP::setParameter
        DSP::getParameter
        FMOD_DSP_TYPE
    ]
    */
    public enum DSP_PITCHSHIFT
    {
        PITCH,       /* Pitch value.  0.5 to 2.0.  Default = 1.0. 0.5 = one octave down, 2.0 = one octave up.  1.0 does not change the pitch. */
        FFTSIZE,     /* FFT window size.  256, 512, 1024, 2048, 4096.  Default = 1024.  Increase this to reduce 'smearing'.  This effect is a warbling sound similar to when an mp3 is encoded at very low bitrates. */
        OVERLAP,     /* Window overlap.  1 to 32.  Default = 4.  Increase this to reduce 'tremolo' effect.  Increasing it by a factor of 2 doubles the CPU usage. */
        MAXCHANNELS  /* Maximum channels supported.  0 to 16.  0 = same as fmod's default output polyphony, 1 = mono, 2 = stereo etc.  See remarks for more.  Default = 0.  It is suggested to leave at 0! */
    }



    /*
    [ENUM]
    [  
        [DESCRIPTION]   
        Parameter types for the FMOD_DSP_TYPE_CHORUS filter.

        [REMARKS]
        Chrous is an effect where the sound is more 'spacious' due to 1 to 3 versions of the sound being played along side the original signal but with the pitch of each copy modulating on a sine wave.<br>
        This is a highly configurable chorus unit.  It supports 3 taps, small and large delay times and also feedback.<br>
        This unit also could be used to do a simple echo, or a flange effect. 

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, XBox, PlayStation 2, GameCube

        [SEE_ALSO]      
        DSP::setParameter
        DSP::getParameter
        FMOD_DSP_TYPE
    ]
    */
    public enum DSP_CHORUS
    {
        DRYMIX,   /* Volume of original signal to pass to output.  0.0 to 1.0. Default = 0.5. */
        WETMIX1,  /* Volume of 1st chorus tap.  0.0 to 1.0.  Default = 0.5. */
        WETMIX2,  /* Volume of 2nd chorus tap. This tap is 90 degrees out of phase of the first tap.  0.0 to 1.0.  Default = 0.5. */
        WETMIX3,  /* Volume of 3rd chorus tap. This tap is 90 degrees out of phase of the second tap.  0.0 to 1.0.  Default = 0.5. */
        DELAY,    /* Chorus delay in ms.  0.1 to 100.0.  Default = 40.0 ms. */
        RATE,     /* Chorus modulation rate in hz.  0.0 to 20.0.  Default = 0.8 hz. */
        DEPTH,    /* Chorus modulation depth.  0.0 to 1.0.  Default = 0.03. */
        FEEDBACK  /* Chorus feedback.  Controls how much of the wet signal gets fed back into the chorus buffer.  0.0 to 1.0.  Default = 0.0. */
    }



    /*
    [ENUM]
    [  
        [DESCRIPTION]   
        Parameter types for the FMOD_DSP_TYPE_REVERB filter.

        [REMARKS]

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable

        [SEE_ALSO]      
        DSP::setParameter
        DSP::getParameter
        FMOD_DSP_TYPE
    ]
    */
    public enum DSP_REVERB
    {
        ROOMSIZE, /* Roomsize. 0.0 to 1.0.  Default = 0.5 */
        DAMP,     /* Damp.     0.0 to 1.0.  Default = 0.5 */
        WETMIX,   /* Wet mix.  0.0 to 1.0.  Default = 0.33 */
        DRYMIX,   /* Dry mix.  0.0 to 1.0.  Default = 0.0 */
        WIDTH,    /* Width.    0.0 to 1.0.  Default = 1.0 */
        MODE      /* Mode.     0 (normal), 1 (freeze).  Default = 0 */
    }

    /*
    [ENUM]
    [  
        [DESCRIPTION]   
        Parameter types for the FMOD_DSP_TYPE_ITECHO filter.<br>
        This is effectively a software based echo filter that emulates the DirectX DMO echo effect.  Impulse tracker files can support this, and FMOD will produce the effect on ANY platform, not just those that support DirectX effects!<br>

        [REMARKS]
        Note.  Every time the delay is changed, the plugin re-allocates the echo buffer.  This means the echo will dissapear at that time while it refills its new buffer.<br>
        Larger echo delays result in larger amounts of memory allocated.<br>
        <br>
        For stereo signals only!  This will not work on mono or multichannel signals.  This is fine for .IT format purposes, and also if you use System::addDSP with a standard stereo output.<br>

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable

        [SEE_ALSO]      
        DSP::setParameter
        DSP::getParameter
        FMOD_DSP_TYPE
        System::addDSP
    ]
    */
    public enum DSP_ITECHO
    {
        WETDRYMIX,      /* Ratio of wet (processed) signal to dry (unprocessed) signal. Must be in the range from 0.0 through 100.0 (all wet). The default value is 50. */
        FEEDBACK,       /* Percentage of output fed back into input, in the range from 0.0 through 100.0. The default value is 50. */
        LEFTDELAY,      /* Delay for left channel, in milliseconds, in the range from 1.0 through 2000.0. The default value is 500 ms. */
        RIGHTDELAY,     /* Delay for right channel, in milliseconds, in the range from 1.0 through 2000.0. The default value is 500 ms. */
        PANDELAY        /* Value that specifies whether to swap left and right delays with each successive echo. The default value is zero, meaning no swap. Possible values are defined as 0.0 (equivalent to FALSE) and 1.0 (equivalent to TRUE). */
    }


    /*
    [ENUM]
    [  
        [DESCRIPTION]   
        Parameter types for the FMOD_DSP_TYPE_COMPRESSOR unit.<br>
        This is a simple linked multichannel software limiter that is uniform across the whole spectrum.<br>

        [REMARKS]
        The parameters are as follows:
        Threshold: [-60dB to 0dB, default 0dB]
        Attack Time: [10ms to 200ms, default 50ms]
        Release Time: [20ms to 1000ms, default 50ms]
        Gain Make Up: [0dB to +30dB, default 0dB]
        <br>
        The limiter is not guaranteed to catch every peak above the threshold level,
        because it cannot apply gain reduction instantaneously - the time delay is
        determined by the attack time. However setting the attack time too short will
        distort the sound, so it is a compromise. High level peaks can be avoided by
        using a short attack time - but not too short, and setting the threshold a few
        decibels below the critical level.
        <br>

        [PLATFORMS]
        Win32, Win64, Linux, Linux64, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable, PlayStation 3

        [SEE_ALSO]      
        DSP::SetParameter
        DSP::GetParameter
        FMOD_DSP_TYPE
        System::addDSP
    ]
    */
    public enum DSP_COMPRESSOR
    {
        THRESHOLD,  /* Threshold level (dB)in the range from -60 through 0. The default value is 50. */ 
        ATTACK,     /* Gain reduction attack time (milliseconds), in the range from 10 through 200. The default value is 50. */    
        RELEASE,    /* Gain reduction release time (milliseconds), in the range from 20 through 1000. The default value is 50. */     
        GAINMAKEUP /* Make-up gain applied after limiting, in the range from 0.0 through 100.0. The default value is 50. */   
    }
}

