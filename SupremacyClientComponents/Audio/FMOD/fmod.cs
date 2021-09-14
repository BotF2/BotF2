
/* ========================================================================================== */
/* FMOD Ex - C# Wrapper . Copyright (c), Firelight Technologies Pty, Ltd. 2004-2005.          */
/*                                                                                            */
/*                                                                                            */
/* ========================================================================================== */

using System;
using System.Text;
using System.Runtime.InteropServices;
using Supremacy.Utility;

namespace FMOD
{
    /*
        FMOD version number.  Check this against FMOD::System::getVersion / System_GetVersion
        0xaaaabbcc -> aaaa = major version number.  bb = minor version number.  cc = development version number.
    */
    public class VERSION
    {
        public const int number = 0x00040308;
        public const string dll = "fmodex.dll";
    }

    /*
        FMOD types 
    */
    /*
    [STRUCTURE] 
    [
        [DESCRIPTION]   
        Structure describing a point in 3D space.

        [REMARKS]
        FMOD uses a left handed co-ordinate system by default.
        To use a right handed co-ordinate system specify FMOD_INIT_3D_RIGHTHANDED from FMOD_INITFLAGS in System::init.

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable

        [SEE_ALSO]      
        System::set3DListenerAttributes
        System::get3DListenerAttributes
        Channel::set3DAttributes
        Channel::get3DAttributes
        Geometry::addPolygon
        Geometry::setPolygonVertex
        Geometry::getPolygonVertex
        Geometry::setRotation
        Geometry::getRotation
        Geometry::setPosition
        Geometry::getPosition
        Geometry::setScale
        Geometry::getScale
        FMOD_INITFLAGS
    ]
    */
    public struct VECTOR
    {
        public float x;        /* X co-ordinate in 3D space. */
        public float y;        /* Y co-ordinate in 3D space. */
        public float z;        /* Z co-ordinate in 3D space. */
    }

    /*
    [ENUM]
    [
        [DESCRIPTION]   
        error codes.  Returned from every function.

        [REMARKS]

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable

        [SEE_ALSO]      
    ]
    */
    public enum RESULT
    {
        OK,                        /* No errors. */
        ERR_ALREADYLOCKED,         /* Tried to call lock a second time before unlock was called. */
        ERR_BADCOMMAND,            /* Tried to call a function on a data design that does not allow this design of functionality (ie calling Sound::lock on a streaming sound). */
        ERR_CDDA_DRIVERS,          /* Neither NTSCSI nor ASPI could be initialised. */
        ERR_CDDA_INIT,             /* An error occurred while initialising the CDDA subsystem. */
        ERR_CDDA_INVALID_DEVICE,   /* Couldn't find the specified device. */
        ERR_CDDA_NOAUDIO,          /* No audio tracks on the specified disc. */
        ERR_CDDA_NODEVICES,        /* No CD/DVD devices were found. */
        ERR_CDDA_NODISC,           /* No disc present in the specified drive. */
        ERR_CDDA_READ,             /* A CDDA read error occurred. */
        ERR_CHANNEL_ALLOC,         /* Error trying to allocate a channel. */
        ERR_CHANNEL_STOLEN,        /* The specified channel has been reused to play another sound. */
        ERR_COM,                   /* A Win32 COM related error occured. COM failed to initialize or a QueryInterface failed meaning a Windows codec or driver was not installed properly. */
        ERR_DMA,                   /* DMA Failure.  See debug output for more information. */
        ERR_DSP_CONNECTION,        /* DSP connection error.  Connection possibly caused a cyclic dependancy. */
        ERR_DSP_FORMAT,            /* DSP Format error.  A DSP unit may have attempted to connect to this network with the wrong format. */
        ERR_DSP_NOTFOUND,          /* DSP connection error.  Couldn't find the DSP unit specified. */
        ERR_DSP_RUNNING,           /* DSP error.  Cannot perform this operation while the network is in the middle of running.  This will most likely happen if a connection or disconnection is attempted in a DSP callback. */
        ERR_DSP_TOOMANYCONNECTIONS,/* DSP connection error.  The unit being connected to or disconnected should only have 1 input or output. */
        ERR_FILE_BAD,              /* Error loading file. */
        ERR_FILE_COULDNOTSEEK,     /* Couldn't perform seek operation.  This is a limitation of the medium (ie netstreams) or the file format. */
        ERR_FILE_EOF,              /* End of file unexpectedly reached while trying to read essential data (truncated data?). */
        ERR_FILE_NOTFOUND,         /* File not found. */
        ERR_FILE_UNWANTED,         /* Unwanted file access occured. */
        ERR_FORMAT,                /* Unsupported file or audio format. */
        ERR_HTTP,                  /* A HTTP error occurred. This is a catch-all for HTTP errors not listed elsewhere. */
        ERR_HTTP_ACCESS,           /* The specified resource requires authentication or is forbidden. */
        ERR_HTTP_PROXY_AUTH,       /* Proxy authentication is required to access the specified resource. */
        ERR_HTTP_SERVER_ERROR,     /* A HTTP server error occurred. */
        ERR_HTTP_TIMEOUT,          /* The HTTP request timed out. */
        ERR_INITIALIZATION,        /* FMOD was not initialized correctly to support this function. */
        ERR_INITIALIZED,           /* Cannot call this command after System::init. */
        ERR_INTERNAL,              /* An error occured that wasnt supposed to.  Contact support. */
        ERR_INVALID_HANDLE,        /* An invalid object handle was used. */
        ERR_INVALID_PARAM,         /* An invalid parameter was passed to this function. */
        ERR_INVALID_SPEAKER,       /* An invalid speaker was passed to this function based on the current speaker mode. */
        ERR_IRX,                   /* PS2 only.  fmodex.irx failed to initialize.  This is most likely because you forgot to load it. */
        ERR_MEMORY,                /* Not enough memory or resources. */
        ERR_MEMORY_IOP,            /* PS2 only.  Not enough memory or resources on PlayStation 2 IOP ram. */
        ERR_MEMORY_SRAM,           /* Not enough memory or resources on console sound ram. */
        ERR_NEEDS2D,               /* Tried to call a command on a 3d sound when the command was meant for 2d sound. */
        ERR_NEEDS3D,               /* Tried to call a command on a 2d sound when the command was meant for 3d sound. */
        ERR_NEEDSHARDWARE,         /* Tried to use a feature that requires hardware support.  (ie trying to play a VAG compressed sound in software on PS2). */
        ERR_NEEDSSOFTWARE,         /* Tried to use a feature that requires the software engine.  Software engine has either been turned off, or command was executed on a hardware channel which does not support this feature. */
        ERR_NET_CONNECT,           /* Couldn't connect to the specified host. */
        ERR_NET_SOCKET_ERROR,      /* A socket error occurred.  This is a catch-all for socket-related errors not listed elsewhere. */
        ERR_NET_URL,               /* The specified URL couldn't be resolved. */
        ERR_NOTREADY,              /* Operation could not be performed because specified sound is not ready. */
        ERR_OUTPUT_ALLOCATED,      /* Error initializing output device, but more specifically, the output device is already in use and cannot be reused. */
        ERR_OUTPUT_CREATEBUFFER,   /* Error creating hardware sound buffer. */
        ERR_OUTPUT_DRIVERCALL,     /* A call to a standard soundcard driver failed, which could possibly mean a bug in the driver or resources were missing or exhausted. */
        ERR_OUTPUT_FORMAT,         /* Soundcard does not support the minimum features needed for this soundsystem (16bit stereo output). */
        ERR_OUTPUT_INIT,           /* Error initializing output device. */
        ERR_OUTPUT_NOHARDWARE,     /* FMOD_HARDWARE was specified but the sound card does not have the resources nescessary to play it. */
        ERR_OUTPUT_NOSOFTWARE,     /* Attempted to create a software sound but no software channels were specified in System::init. */
        ERR_PAN,                   /* Panning only works with mono or stereo sound sources. */
        ERR_PLUGIN,                /* An unspecified error has been returned from a 3rd party plugin. */
        ERR_PLUGIN_MISSING,        /* A requested output, dsp unit design or codec was not available. */
        ERR_PLUGIN_RESOURCE,       /* A resource that the plugin requires cannot be found. (ie the DLS file for MIDI playback) */
        ERR_RECORD,                /* An error occured trying to initialize the recording device. */
        ERR_REVERB_INSTANCE,       /* Specified Instance in FMOD_REVERB_PROPERTIES couldn't be set. Most likely because another application has locked the EAX4 FX slot. */
        ERR_SUBSOUND_ALLOCATED,    /* This subsound is already being used by another sound, you cannot have more than one parent to a sound.  Null out the other parent's entry first. */
        ERR_TAGNOTFOUND,           /* The specified tag could not be found or there are no tags. */
        ERR_TOOMANYCHANNELS,       /* The sound created exceeds the allowable input channel count.  This can be increased with System::setMaxInputChannels. */
        ERR_UNIMPLEMENTED,         /* Something in FMOD hasn't been implemented when it should be! contact support! */
        ERR_UNINITIALIZED,         /* This command failed because System::init or System::setDriver was not called. */
        ERR_UNSUPPORTED,           /* A command issued was not supported by this object.  Possibly a plugin without certain callbacks specified. */
        ERR_UPDATE,                /* On PS2, System::update was called twice in a row when System::updateFinished must be called first. */
        ERR_VERSION,               /* The version number of this file format is not supported. */
    }



    /*
    [ENUM]
    [
        [DESCRIPTION]   
        These output types are used with System::setOutput/System::getOutput, to choose which output method to use.
  
        [REMARKS]
        To drive the output synchronously, and to disable FMOD's timing thread, use the FMOD_INIT_NONREALTIME flag.
        
        To pass information to the driver when initializing fmod use the extradriverdata parameter for the following reasons.
        <li>FMOD_OUTPUTTYPE_WAVWRITER - extradriverdata is a pointer to a char * filename that the wav writer will output to.
        <li>FMOD_OUTPUTTYPE_WAVWRITER_NRT - extradriverdata is a pointer to a char * filename that the wav writer will output to.
        <li>FMOD_OUTPUTTYPE_DSOUND - extradriverdata is a pointer to a HWND so that FMOD can set the focus on the audio for a particular window.
        <li>FMOD_OUTPUTTYPE_GC - extradriverdata is a pointer to a FMOD_ARAMBLOCK_INFO struct. This can be found in fmodgc.h.
        Currently these are the only FMOD drivers that take extra information.  Other unknown plugins may have different requirements.

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable

        [SEE_ALSO]      
        System::setOutput
        System::getOutput
        System::setSoftwareFormat
        System::getSoftwareFormat
        System::init
    ]
    */
    public enum OUTPUTTYPE
    {
        AUTODETECT,    /* Picks the best output mode for the platform.  This is the default. */

        UNKNOWN,       /* All         - 3rd party plugin, unknown.  This is for use with System::getOutput only. */
        NOSOUND,       /* All         - All calls in this mode succeed but make no sound. */
        WAVWRITER,     /* All         - All         - Writes output to fmodout.wav by default.  Use System::setSoftwareFormat to set the filename. */
        NOSOUND_NRT,   /* All         - Non-realtime version of FMOD_OUTPUTTYPE_NOSOUND.  User can drive mixer with System::update at whatever rate they want. */
        WAVWRITER_NRT, /* All         - Non-realtime version of FMOD_OUTPUTTYPE_WAVWRITER.  User can drive mixer with System::update at whatever rate they want. */

        DSOUND,        /* Win32/Win64 - DirectSound output.  Use this to get hardware accelerated 3d audio and EAX Reverb support. (Default on Windows) */
        WINMM,         /* Win32/Win64 - Windows Multimedia output. */
        ASIO,          /* Win32       - Low latency ASIO driver. */
        OSS,           /* Linux       - Open Sound System output. */
        ALSA,          /* Linux       - Advanced Linux Sound Architecture output. */
        ESD,           /* Linux       - Enlightment Sound Daemon output. */
        SOUNDMANAGER,  /* Mac         - Macintosh SoundManager output. */
        COREAUDIO,     /* Mac         - Macintosh CoreAudio output */
        XBOX,          /* Xbox        - Native hardware output. */
        PS2,           /* PS2         - Native hardware output. */
        GC,            /* GameCube    - Native hardware output. */
        XBOX360,       /* Xbox 360    - Native hardware output. */
        PSP,           /* PSP         - Native hardware output. */
        OPENAL,        /* Win32/Win64 - OpenAL 1.1 output. Use this for lower CPU overhead than FMOD_OUTPUTTYPE_DSOUND, and also Vista H/W support with Creative Labs cards. */

        MAX            /* Maximum number of output types supported. */
    }


    /*
    [ENUM] 
    [
        [DESCRIPTION]   

        [REMARKS]

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable

        [SEE_ALSO]
    ]
    */
    public enum CAPS
    {
        NONE = 0x00000000,    /* Device has no special capabilities. */
        HARDWARE = 0x00000001,    /* Device supports hardware mixing. */
        HARDWARE_EMULATED = 0x00000002,    /* Device supports FMOD_HARDWARE but it will be mixed on the CPU by the kernel (not FMOD's software mixer). */
        OUTPUT_MULTICHANNEL = 0x00000004,    /* Device can do multichannel output, ie greater than 2 channels. */
        OUTPUT_FORMAT_PCM8 = 0x00000008,    /* Device can output to 8bit integer PCM. */
        OUTPUT_FORMAT_PCM16 = 0x00000010,    /* Device can output to 16bit integer PCM. */
        OUTPUT_FORMAT_PCM24 = 0x00000020,    /* Device can output to 24bit integer PCM. */
        OUTPUT_FORMAT_PCM32 = 0x00000040,    /* Device can output to 32bit integer PCM. */
        OUTPUT_FORMAT_PCMFLOAT = 0x00000080,    /* Device can output to 32bit floating point PCM. */
        REVERB_EAX2 = 0x00000100,    /* Device supports EAX2 reverb. */
        REVERB_EAX3 = 0x00000200,    /* Device supports EAX3 reverb. */
        REVERB_EAX4 = 0x00000400,    /* Device supports EAX4 reverb  */
        REVERB_I3DL2 = 0x00000800,    /* Device supports I3DL2 reverb. */
        REVERB_LIMITED = 0x00001000     /* Device supports some form of limited hardware reverb, maybe parameterless and only selectable by environment. */
    }


    /*
    [ENUM]
    [
        [DESCRIPTION]   
        These are speaker types defined for use with the System::setSpeakerMode or System::getSpeakerMode command.

        [REMARKS]
        These are important notes on speaker modes in regards to sounds created with FMOD_SOFTWARE.<br>
        Note below the phrase 'sound channels' is used.  These are the subchannels inside a sound, they are not related and 
        have nothing to do with the FMOD class "Channel".<br>
        For example a mono sound has 1 sound channel, a stereo sound has 2 sound channels, and an AC3 or 6 channel wav file have 6 "sound channels".<br>
        <br>
        FMOD_SPEAKERMODE_RAW<br>
        ---------------------<br>
        This mode is for output devices that are not specifically mono/stereo/quad/surround/5.1 or 7.1, but are multichannel.<br>
        Sound channels map to speakers sequentially, so a mono sound maps to output speaker 0, stereo sound maps to output speaker 0 & 1.<br>
        The user assumes knowledge of the speaker order.  FMOD_SPEAKER enumerations may not apply, so raw channel indicies should be used.<br>
        Multichannel sounds map input channels to output channels 1:1. <br>
        Channel::setPan and Channel::setSpeakerMix do not work.<br>
        Speaker levels must be manually set with Channel::setSpeakerLevels.<br>
        <br>
        FMOD_SPEAKERMODE_MONO<br>
        ---------------------<br>
        This mode is for a 1 speaker arrangement.<br>
        Panning does not work in this speaker mode.<br>
        Mono, stereo and multichannel sounds have each sound channel played on the one speaker unity.<br>
        Mix behaviour for multichannel sounds can be set with Channel::setSpeakerLevels.<br>
        Channel::setSpeakerMix does not work.<br>
        <br>
        FMOD_SPEAKERMODE_STEREO<br>
        -----------------------<br>
        This mode is for 2 speaker arrangements that have a left and right speaker.<br>
        <li>Mono sounds default to an even distribution between left and right.  They can be panned with Channel::setPan.<br>
        <li>Stereo sounds default to the middle, or full left in the left speaker and full right in the right speaker.  
        <li>They can be cross faded with Channel::setPan.<br>
        <li>Multichannel sounds have each sound channel played on each speaker at unity.<br>
        <li>Mix behaviour for multichannel sounds can be set with Channel::setSpeakerLevels.<br>
        <li>Channel::setSpeakerMix works but only front left and right parameters are used, the rest are ignored.<br>
        <br>
        FMOD_SPEAKERMODE_QUAD<br>
        ------------------------<br>
        This mode is for 4 speaker arrangements that have a front left, front right, rear left and a rear right speaker.<br>
        <li>Mono sounds default to an even distribution between front left and front right.  They can be panned with Channel::setPan.<br>
        <li>Stereo sounds default to the left sound channel played on the front left, and the right sound channel played on the front right.<br>
        <li>They can be cross faded with Channel::setPan.<br>
        <li>Multichannel sounds default to all of their sound channels being played on each speaker in order of input.<br>
        <li>Mix behaviour for multichannel sounds can be set with Channel::setSpeakerLevels.<br>
        <li>Channel::setSpeakerMix works but side left, side right, center and lfe are ignored.<br>
        <br>
        FMOD_SPEAKERMODE_SURROUND<br>
        ------------------------<br>
        This mode is for 4 speaker arrangements that have a front left, front right, front center and a rear center.<br>
        <li>Mono sounds default to the center speaker.  They can be panned with Channel::setPan.<br>
        <li>Stereo sounds default to the left sound channel played on the front left, and the right sound channel played on the front right.  
        <li>They can be cross faded with Channel::setPan.<br>
        <li>Multichannel sounds default to all of their sound channels being played on each speaker in order of input.<br>
        <li>Mix behaviour for multichannel sounds can be set with Channel::setSpeakerLevels.<br>
        <li>Channel::setSpeakerMix works but side left, side right and lfe are ignored, and rear left / rear right are averaged into the rear speaker.<br>
        <br>
        FMOD_SPEAKERMODE_5POINT1<br>
        ------------------------<br>
        This mode is for 5.1 speaker arrangements that have a left/right/center/rear left/rear right and a subwoofer speaker.<br>
        <li>Mono sounds default to the center speaker.  They can be panned with Channel::setPan.<br>
        <li>Stereo sounds default to the left sound channel played on the front left, and the right sound channel played on the front right.  
        <li>They can be cross faded with Channel::setPan.<br>
        <li>Multichannel sounds default to all of their sound channels being played on each speaker in order of input.  
        <li>Mix behaviour for multichannel sounds can be set with Channel::setSpeakerLevels.<br>
        <li>Channel::setSpeakerMix works but side left / side right are ignored.<br>
        <br>
        FMOD_SPEAKERMODE_7POINT1<br>
        ------------------------<br>
        This mode is for 7.1 speaker arrangements that have a left/right/center/rear left/rear right/side left/side right 
        and a subwoofer speaker.<br>
        <li>Mono sounds default to the center speaker.  They can be panned with Channel::setPan.<br>
        <li>Stereo sounds default to the left sound channel played on the front left, and the right sound channel played on the front right.  
        <li>They can be cross faded with Channel::setPan.<br>
        <li>Multichannel sounds default to all of their sound channels being played on each speaker in order of input.  
        <li>Mix behaviour for multichannel sounds can be set with Channel::setSpeakerLevels.<br>
        <li>Channel::setSpeakerMix works and every parameter is used to set the balance of a sound in any speaker.<br>
        <br>
        FMOD_SPEAKERMODE_PROLOGIC<br>
        ------------------------------------------------------<br>
        This mode is for mono, stereo, 5.1 and 7.1 speaker arrangements, as it is backwards and forwards compatible with stereo, 
        but to get a surround effect a Dolby Prologic or Prologic 2 hardware decoder / amplifier is needed.<br>
        Pan behaviour is the same as FMOD_SPEAKERMODE_5POINT1.<br>
        <br>
        If this function is called the numoutputchannels setting in System::setSoftwareFormat is overwritten.<br>
        <br>
        For 3D sounds, panning is determined at runtime by the 3D subsystem based on the speaker mode to determine which speaker the 
        sound should be placed in.<br>

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable

        [SEE_ALSO]
        System::setSpeakerMode
        System::getSpeakerMode
        System::getDriverCaps
        Channel::setSpeakerLevels
    ]
    */
    public enum SPEAKERMODE
    {
        RAW,              /* There is no specific speakermode.  Sound channels are mapped in order of input to output.  See remarks for more information. */
        MONO,             /* The speakers are monaural. */
        STEREO,           /* The speakers are stereo (DEFAULT). */
        QUAD,             /* 4 speaker setup.  This includes front left, front right, rear left, rear right.  */
        SURROUND,         /* 4 speaker setup.  This includes front left, front right, center, rear center (rear left/rear right are averaged). */
        _5POINT1,         /* 5.1 speaker setup.  This includes front left, front right, center, rear left, rear right and a subwoofer. */
        _7POINT1,         /* 7.1 speaker setup.  This includes front left, front right, center, rear left, rear right, side left, side right and a subwoofer. */
        PROLOGIC,         /* Stereo output, but data is encoded in a way that is picked up by a Prologic/Prologic2 decoder and split into a 5.1 speaker setup. */

        FMOD_SPEAKERMODE_MAX,              /* Maximum number of speaker modes supported. */
    }


    /*
    [ENUM]
    [
        [DESCRIPTION]   
        These are speaker types defined for use with the Channel::setSpeakerLevels command.
        It can also be used for speaker placement in the System::setSpeakerPosition command.

        [REMARKS]
        If you are using FMOD_SPEAKERMODE_RAW and speaker assignments are meaningless, just cast a raw integer value to this design.<br>
        For example (FMOD_SPEAKER)7 would use the 7th speaker (also the same as FMOD_SPEAKER_SIDE_RIGHT).<br>
        Values higher than this can be used if an output system has more than 8 speaker types / output channels.  15 is the current maximum.

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable

        [SEE_ALSO]
        FMOD_SPEAKERMODE
        Channel::setSpeakerLevels
        Channel::getSpeakerLevels
        System::setSpeakerPosition
        System::getSpeakerPosition
    ]
    */
    public enum SPEAKER
    {
        FRONT_LEFT,
        FRONT_RIGHT,
        FRONT_CENTER,
        LOW_FREQUENCY,
        BACK_LEFT,
        BACK_RIGHT,
        SIDE_LEFT,
        SIDE_RIGHT,

        MAX,                          /* Maximum number of speaker types supported. */
        MONO = FRONT_LEFT,     /* For use with FMOD_SPEAKERMODE_MONO and Channel::SetSpeakerLevels.  Mapped to same value as FMOD_SPEAKER_FRONT_LEFT. */
        BACK_CENTER = LOW_FREQUENCY,  /* For use with FMOD_SPEAKERMODE_SURROUND and Channel::SetSpeakerLevels only.  Mapped to same value as FMOD_SPEAKER_LOW_FREQUENCY. */
    }


    /*
    [ENUM]
    [
        [DESCRIPTION]   
        These are plugin types defined for use with the System::getNumPlugins / System_GetNumPlugins, 
        System::getPluginInfo / System_GetPluginInfo and System::unloadPlugin / System_UnloadPlugin functions.

        [REMARKS]

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable

        [SEE_ALSO]
        System::getNumPlugins
        System::getPluginInfo
        System::unloadPlugin
    ]
    */
    public enum PLUGINTYPE
    {
        OUTPUT,     /* The plugin design is an output module.  FMOD mixed audio will play through one of these devices */
        CODEC,      /* The plugin design is a file format codec.  FMOD will use these codecs to load file formats for playback. */
        DSP         /* The plugin design is a DSP unit.  FMOD will use these plugins as part of its DSP network to apply effects to output or generate sound in realtime. */
    }


    /*
    [ENUM] 
    [
        [DESCRIPTION]   
        Initialization flags.  Use them with System::init in the flags parameter to change various behaviour.  

        [REMARKS]

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable

        [SEE_ALSO]
        System::init
    ]
    */
    public enum INITFLAG
    {
        NORMAL = 0x00000000,   /* All platforms - Initialize normally */
        STREAM_FROM_UPDATE = 0x00000001,   /* All platforms - No stream thread is created internally.  Streams are driven from System::update.  Mainly used with non-realtime outputs. */
        _3D_RIGHTHANDED = 0x00000002,   /* All platforms - FMOD will treat +X as left, +Y as up and +Z as forwards. */
        DISABLESOFTWARE = 0x00000004,   /* All platforms - Disable software mixer to save memory.  Anything created with FMOD_SOFTWARE will fail and DSP will not work. */
        DSOUND_HRTFNONE = 0x00000200,   /* Win32 only - for DirectSound output - FMOD_HARDWARE | FMOD_3D buffers use simple stereo panning/doppler/attenuation when 3D hardware acceleration is not present. */
        DSOUND_HRTFLIGHT = 0x00000400,   /* Win32 only - for DirectSound output - FMOD_HARDWARE | FMOD_3D buffers use a slightly higher quality algorithm when 3D hardware acceleration is not present. */
        DSOUND_HRTFFULL = 0x00000800,   /* Win32 only - for DirectSound output - FMOD_HARDWARE | FMOD_3D buffers use full quality 3D playback when 3d hardware acceleration is not present. */
        PS2_DISABLECORE0REVERB = 0x00010000,   /* PS2 only - Disable reverb on CORE 0 to regain SRAM. */
        PS2_DISABLECORE1REVERB = 0x00020000,   /* PS2 only - Disable reverb on CORE 1 to regain SRAM. */
        XBOX_REMOVEHEADROOM = 0x00100000,   /* XBox only - By default DirectSound attenuates all sound by 6db to avoid clipping/distortion.  CAUTION.  If you use this flag you are responsible for the final mix to make sure clipping / distortion doesn't happen. */
    }


    /*
    [ENUM]
    [
        [DESCRIPTION]   
        These definitions describe the design of song being played.

        [REMARKS]

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable

        [SEE_ALSO]      
        Sound::getFormat
    ]
    */
    public enum SOUND_TYPE
    {
        UNKNOWN,    /* 3rd party / unknown plugin format. */
        AIFF,       /* AIFF */
        ASF,        /* Microsoft Advanced Systems Format (ie WMA/ASF/WMV) */
        AAC,        /* AAC */
        CDDA,       /* Digital CD audio */
        DLS,        /* Sound font / downloadable sound bank. */
        FLAC,       /* FLAC lossless codec. */
        FSB,        /* FMOD Sample Bank */
        GCADPCM,    /* GameCube ADPCM */
        IT,         /* Impulse Tracker. */
        MIDI,       /* MIDI */
        MOD,        /* Protracker / Fasttracker MOD. */
        MPEG,       /* MP2/MP3 MPEG. */
        OGGVORBIS,  /* Ogg vorbis. */
        PLAYLIST,   /* Information only from ASX/PLS/M3U/WAX playlists */
        S3M,        /* ScreamTracker 3. */
        SF2,        /* Sound font 2 format. */
        RAW,        /* Raw PCM data. */
        USER,       /* User created sound */
        WAV,        /* Microsoft WAV. */
        XM          /* FastTracker 2 XM. */
    }


    /*
    [ENUM]
    [
        [DESCRIPTION]   
        These definitions describe the native format of the hardware or software buffer that will be used.

        [REMARKS]
        This is the format the native hardware or software buffer will be or is created in.

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable

        [SEE_ALSO]
        System::createSoundEx
        Sound::getFormat
    ]
    */
    public enum SOUND_FORMAT
    {
        NONE,     /* Unitialized / unknown */
        PCM8,     /* 8bit integer PCM data */
        PCM16,    /* 16bit integer PCM data  */
        PCM24,    /* 24bit integer PCM data  */
        PCM32,    /* 32bit integer PCM data  */
        PCMFLOAT, /* 32bit floating point PCM data  */
        GCADPCM,  /* Compressed GameCube DSP data */
        IMAADPCM, /* Compressed XBox ADPCM data */
        VAG,      /* Compressed PlayStation 2 ADPCM data */
        XMA,      /* Compressed Xbox360 data. */
        MPEG,     /* Compressed MPEG layer 2 or 3 data. */
        MAX       /* Maximum number of sound formats supported. */
    }


    /*
	[DEFINE]
	[
		[NAME] 
		FMOD_MODE

		[DESCRIPTION]   
		Sound description bitfields, bitwise OR them together for loading and describing sounds.

		[REMARKS]
		By default a sound will open as a static sound that is decompressed fully into memory.<br>
		To have a sound stream instead, use FMOD_CREATESTREAM.<br>
		Some opening modes (ie FMOD_OPENUSER, FMOD_OPENMEMORY, FMOD_OPENRAW) will need extra information.<br>
		This can be provided using the FMOD_CREATESOUNDEXINFO structure.

		[PLATFORMS]
		Win32, Win64, Linux, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable

		[SEE_ALSO]
		System::createSound
		System::createStream
		Sound::setMode
		Sound::getMode
		Channel::setMode
		Channel::getMode
		Sound::set3DCustomRolloff
        Channel::set3DCustomRolloff
	]
	*/
    [Flags]
    public enum MODE
    {
        DEFAULT = 0x00000000,  /* FMOD_DEFAULT is a default sound design.  Equivalent to all the defaults listed below.  FMOD_LOOP_OFF, FMOD_2D, FMOD_HARDWARE. */
        LOOP_OFF = 0x00000001,  /* For non looping sounds. (default).  Overrides FMOD_LOOP_NORMAL / FMOD_LOOP_BIDI. */
        LOOP_NORMAL = 0x00000002,  /* For forward looping sounds. */
        LOOP_BIDI = 0x00000004,  /* For bidirectional looping sounds. (only works on software mixed static sounds). */
        _2D = 0x00000008,  /* Ignores any 3d processing. (default). */
        _3D = 0x00000010,  /* Makes the sound positionable in 3D.  Overrides FMOD_2D. */
        HARDWARE = 0x00000020,  /* Attempts to make sounds use hardware acceleration. (default). */
        SOFTWARE = 0x00000040,  /* Makes sound reside in software.  Overrides FMOD_HARDWARE.  Use this for FFT, DSP, 2D multi speaker support and other software related features. */
        CREATESTREAM = 0x00000080,  /* Decompress at runtime, streaming from the source provided (standard stream).  Overrides FMOD_CREATESAMPLE. */
        CREATESAMPLE = 0x00000100,  /* Decompress at loadtime, decompressing or decoding whole file into memory as the target sample format. (standard sample). */
        CREATECOMPRESSEDSAMPLE = 0x00000200,  /* Load MP2, MP3, IMAADPCM or XMA into memory and leave it compressed.  During playback the FMOD software mixer will decode it in realtime as a 'compressed sample'.  Can only be used in combination with FMOD_SOFTWARE. */
        OPENUSER = 0x00000400,  /* Opens a user created static sample or stream. Use FMOD_CREATESOUNDEXINFO to specify format and/or read callbacks.  If a user created 'sample' is created with no read callback, the sample will be empty.  Use FMOD_Sound_Lock and FMOD_Sound_Unlock to place sound data into the sound if this is the case. */
        OPENMEMORY = 0x00000800,  /* "name_or_data" will be interpreted as a pointer to memory instead of filename for creating sounds. */
        OPENRAW = 0x00001000,  /* Will ignore file format and treat as raw pcm.  User may need to declare if data is FMOD_SIGNED or FMOD_UNSIGNED */
        OPENONLY = 0x00002000,  /* Just open the file, dont prebuffer or read.  Good for fast opens for info, or when sound::readData is to be used. */
        ACCURATETIME = 0x00004000,  /* For FMOD_CreateSound - for accurate FMOD_Sound_GetLength / FMOD_Channel_SetPosition on VBR MP3, AAC and MOD/S3M/XM/IT/MIDI files.  Scans file first, so takes longer to open. FMOD_OPENONLY does not affect this. */
        MPEGSEARCH = 0x00008000,  /* For corrupted / bad MP3 files.  This will search all the way through the file until it hits a valid MPEG header.  Normally only searches for 4k. */
        NONBLOCKING = 0x00010000,  /* For opening sounds asyncronously, return value from open function must be polled for when it is ready. */
        UNIQUE = 0x00020000,  /* Unique sound, can only be played one at a time */
        _3D_HEADRELATIVE = 0x00040000,  /* Make the sound's position, velocity and orientation relative to the listener. */
        _3D_WORLDRELATIVE = 0x00080000,  /* Make the sound's position, velocity and orientation absolute (relative to the world). (DEFAULT) */
        _3D_LOGROLLOFF = 0x00100000,  /* This sound will follow the standard logarithmic rolloff model where mindistance = full volume, maxdistance = where sound stops attenuating, and rolloff is fixed according to the global rolloff factor.  (default) */
        _3D_LINEARROLLOFF = 0x00200000,  /* This sound will follow a linear rolloff model where mindistance = full volume, maxdistance = silence.  */
        _3D_CUSTOMROLLOFF = 0x04000000,  /* This sound will follow a rolloff model defined by Sound::set3DCustomRolloff / Channel::set3DCustomRolloff.  */
        CDDA_FORCEASPI = 0x00400000,  /* For CDDA sounds only - use ASPI instead of NTSCSI to access the specified CD/DVD device. */
        CDDA_JITTERCORRECT = 0x00800000,  /* For CDDA sounds only - perform jitter correction. Jitter correction helps produce a more accurate CDDA stream at the cost of more CPU time. */
        UNICODE = 0x01000000,  /* Filename is double-byte unicode. */
        IGNORETAGS = 0x02000000,  /* Skips id3v2/asf/etc tag checks when opening a sound, to reduce seek/read overhead when opening files (helps with CD performance). */
        LOWMEM = 0x04000000,  /* Removes some features from samples to give a lower memory overhead, like Sound::getName. */
    }


    /*
    [ENUM]
    [
        [DESCRIPTION]   
        These values describe what state a sound is in after NONBLOCKING has been used to open it.

        [REMARKS]    

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable

        [SEE_ALSO]
        Sound::getOpenState
        MODE
    ]
    */
    public enum OPENSTATE
    {
        READY = 0,       /* Opened and ready to play */
        LOADING,         /* Initial load in progress */
        ERROR,           /* Failed to open - file not found, out of memory etc.  See return value of Sound::getOpenState for what happened. */
        CONNECTING,      /* Connecting to remote host (internet sounds only) */
        BUFFERING        /* Buffering data */
    }


    /*
    [ENUM]
    [
        [DESCRIPTION]   
        These callback types are used with Channel::setCallback.

        [REMARKS]
        Each callback has commanddata parameters passed int unique to the design of callback.
        See reference to FMOD_CHANNEL_CALLBACK to determine what they might mean for each design of callback.

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable

        [SEE_ALSO]      
        Channel::setCallback
        FMOD_CHANNEL_CALLBACK
    ]
    */
    public enum CHANNEL_CALLBACKTYPE
    {
        END,                  /* Called when a sound ends. */
        VIRTUALVOICE,         /* Called when a voice is swapped out or swapped in. */
        SYNCPOINT,            /* Called when a syncpoint is encountered.  Can be from wav file markers. */

        MAX
    }


    /*
    [ENUM]
    [
        [DESCRIPTION]   
        List of windowing methods used in spectrum analysis to reduce leakage / transient signals intefering with the analysis.
        This is a problem with analysis of continuous signals that only have a small portion of the signal sample (the fft window size).
        Windowing the signal with a curve or triangle tapers the sides of the fft window to help alleviate this problem.

        [REMARKS]
        Cyclic signals such as a sine wave that repeat their cycle in a multiple of the window size do not need windowing.
        I.e. If the sine wave repeats every 1024, 512, 256 etc samples and the FMOD fft window is 1024, then the signal would not need windowing.
        Not windowing is the same as FMOD_DSP_FFT_WINDOW_RECT, which is the default.
        If the cycle of the signal (ie the sine wave) is not a multiple of the window size, it will cause frequency abnormalities, so a different windowing method is needed.
        <exclude>
        
        FMOD_DSP_FFT_WINDOW_RECT.
        <img src = "rectangle.gif"></img>
        
        FMOD_DSP_FFT_WINDOW_TRIANGLE.
        <img src = "triangle.gif"></img>
        
        FMOD_DSP_FFT_WINDOW_HAMMING.
        <img src = "hamming.gif"></img>
        
        FMOD_DSP_FFT_WINDOW_HANNING.
        <img src = "hanning.gif"></img>
        
        FMOD_DSP_FFT_WINDOW_BLACKMAN.
        <img src = "blackman.gif"></img>
        
        FMOD_DSP_FFT_WINDOW_BLACKMANHARRIS.
        <img src = "blackmanharris.gif"></img>
        </exclude>

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable

        [SEE_ALSO]      
        System::getSpectrum
        Channel::getSpectrum
    ]
    */
    public enum DSP_FFT_WINDOW
    {
        RECT,           /* w[n] = 1.0                                                                                            */
        TRIANGLE,       /* w[n] = TRI(2n/N)                                                                                      */
        HAMMING,        /* w[n] = 0.54 - (0.46 * COS(n/N) )                                                                      */
        HANNING,        /* w[n] = 0.5 *  (1.0  - COS(n/N) )                                                                      */
        BLACKMAN,       /* w[n] = 0.42 - (0.5  * COS(n/N) ) + (0.08 * COS(2.0 * n/N) )                                           */
        BLACKMANHARRIS, /* w[n] = 0.35875 - (0.48829 * COS(1.0 * n/N)) + (0.14128 * COS(2.0 * n/N)) - (0.01168 * COS(3.0 * n/N)) */

        MAX
    }


    /*
    [ENUM]
    [
        [DESCRIPTION]   
        List of interpolation types that the FMOD Ex software mixer supports.  

        [REMARKS]
        The default resampler design is FMOD_DSP_RESAMPLER_LINEAR.<br>
        Use System::setSoftwareFormat to tell FMOD the resampling quality you require for FMOD_SOFTWARE based sounds.

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable

        [SEE_ALSO]      
        System::setSoftwareFormat
        System::getSoftwareFormat
    ]
    */
    public enum DSP_RESAMPLER
    {
        NOINTERP,        /* No interpolation.  High frequency aliasing hiss will be audible depending on the sample rate of the sound. */
        LINEAR,          /* Linear interpolation (default method).  Fast and good quality, causes very slight lowpass effect on low frequency sounds. */
        CUBIC,           /* Cubic interoplation.  Slower than linear interpolation but better quality. */
        SPLINE,          /* 5 point spline interoplation.  Slowest resampling method but best quality. */

        MAX,             /* Maximum number of resample methods supported. */
    }


    /*
    [ENUM]
    [
        [DESCRIPTION]   
        List of tag types that could be stored within a sound.  These include id3 tags, metadata from netstreams and vorbis/asf data.

        [REMARKS]

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable

        [SEE_ALSO]      
        Sound::getTag
    ]
    */
    public enum TAGTYPE
    {
        UNKNOWN = 0,
        ID3V1,
        ID3V2,
        VORBISCOMMENT,
        SHOUTCAST,
        ICECAST,
        ASF,
        MIDI,
        PLAYLIST,
        FMOD,
        USER
    }


    /*
    [ENUM]
    [
        [DESCRIPTION]   
        List of data types that can be returned by Sound::getTag

        [REMARKS]

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable

        [SEE_ALSO]      
        Sound::getTag
    ]
    */
    public enum TAGDATATYPE
    {
        BINARY = 0,
        INT,
        FLOAT,
        STRING,
        STRING_UTF16,
        STRING_UTF16BE,
        STRING_UTF8,
        CDTOC
    }


    /*
    [STRUCTURE] 
    [
        [DESCRIPTION]   
        Structure describing a piece of tag data.

        [REMARKS]
        Members marked with [in] mean the user sets the value before passing it to the function.
        Members marked with [out] mean FMOD sets the value to be used after the function exits.

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable

        [SEE_ALSO]      
        Sound::getTag
        TAGTYPE
        TAGDATATYPE
    ]
    */
    public struct TAG
    {
        public TAGTYPE type;         /* [out] The design of this tag. */
        public TAGDATATYPE datatype;     /* [out] The design of data that this tag contains */
        public string name;         /* [out] The name of this tag i.e. "TITLE", "ARTIST" etc. */
        public IntPtr data;         /* [out] Pointer to the tag data - its format is determined by the datatype member */
        public uint datalen;      /* [out] Length of the data contained in this tag */
        public bool updated;      /* [out] True if this tag has been updated since last being accessed with Sound::getTag */
    }


    /*
    [STRUCTURE] 
    [
        [DESCRIPTION]   
        Structure describing a CD/DVD table of contents

        [REMARKS]
        Members marked with [in] mean the user sets the value before passing it to the function.
        Members marked with [out] mean FMOD sets the value to be used after the function exits.

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable

        [SEE_ALSO]      
        Sound::getTag
    ]
    */
    public struct CDTOC
    {
        public int numtracks;                  /* [out] The number of tracks on the CD */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
        public int[] min;                   /* [out] The start offset of each track in minutes */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
        public int[] sec;                   /* [out] The start offset of each track in seconds */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
        public int[] frame;                 /* [out] The start offset of each track in frames */
    }


    /*
    [ENUM]
    [
        [DESCRIPTION]   
        List of time types that can be returned by Sound::getLength and used with Channel::setPosition or Channel::getPosition.

        [REMARKS]

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable

        [SEE_ALSO]      
        Sound::getLength
        Channel::setPosition
        Channel::getPosition
    ]
    */
    public enum TIMEUNIT
    {
        MS = 0x00000001,  /* Milliseconds. */
        PCM = 0x00000002,  /* PCM Samples, related to milliseconds * samplerate / 1000. */
        PCMBYTES = 0x00000004,  /* Bytes, related to PCM samples * channels * datawidth (ie 16bit = 2 bytes). */
        RAWBYTES = 0x00000008,  /* Raw file bytes of (compressed) sound data (does not include headers).  Only used by Sound::getLength and Channel::getPosition. */
        MODORDER = 0x00000100,  /* MOD/S3M/XM/IT.  Order in a sequenced module format.  Use Sound::getFormat to determine the format. */
        MODROW = 0x00000200,  /* MOD/S3M/XM/IT.  Current row in a sequenced module format.  Sound::getLength will return the number if rows in the currently playing or seeked to pattern. */
        MODPATTERN = 0x00000400,  /* MOD/S3M/XM/IT.  Current pattern in a sequenced module format.  Sound::getLength will return the number of patterns in the song and Channel::getPosition will return the currently playing pattern. */
        SENTENCE_MS = 0x00010000,  /* Currently playing subsound in a sentence time in milliseconds. */
        SENTENCE_PCM = 0x00020000,  /* Currently playing subsound in a sentence time in PCM Samples, related to milliseconds * samplerate / 1000. */
        SENTENCE_PCMBYTES = 0x00040000,  /* Currently playing subsound in a sentence time in bytes, related to PCM samples * channels * datawidth (ie 16bit = 2 bytes). */
        SENTENCE = 0x00080000,  /* Currently playing sentence index according to the channel. */
        SENTENCE_SUBSOUND = 0x00100000,  /* Currently playing subsound index in a sentence. */
        BUFFERED = 0x10000000,  /* Time value as seen by buffered stream.  This is always ahead of audible time, and is only used for processing. */
    }

    public delegate RESULT SOUND_NONBLOCKCALLBACK(IntPtr soundraw, RESULT result);
    public delegate RESULT SOUND_PCMREADCALLBACK(IntPtr soundraw, IntPtr data, uint datalen);
    public delegate RESULT SOUND_PCMSETPOSCALLBACK(IntPtr soundraw, int subsound, uint position, TIMEUNIT postype);


    /*
    [STRUCTURE] 
    [
        [DESCRIPTION]
        Use this structure with System::createSound when more control is needed over loading.
        The possible reasons to use this with System::createSound are:
        <li>Loading a file from memory.
        <li>Loading a file from within another larger (possibly wad/pak) file, by giving the loader an offset and length.
        <li>To create a user created / non file based sound.
        <li>To specify a starting subsound to seek to within a multi-sample sounds (ie FSB/DLS/SF2) when created as a stream.
        <li>To specify which subsounds to load for multi-sample sounds (ie FSB/DLS/SF2) so that memory is saved and only a subset is actually loaded/read from disk.
        <li>To specify 'piggyback' read and seek callbacks for capture of sound data as fmod reads and decodes it.  Useful for ripping decoded PCM data from sounds as they are loaded / played.
        <li>To specify a MIDI DLS/SF2 sample set file to load when opening a MIDI file.
        See below on what members to fill for each of the above types of sound you want to create.

        [REMARKS]
        This structure is optional!  Specify 0 or NULL in System::createSound if you don't need it!
        
        Members marked with [in] mean the user sets the value before passing it to the function.
        Members marked with [out] mean FMOD sets the value to be used after the function exits.
        
        <u>Loading a file from memory.</u>
        <li>Create the sound using the FMOD_OPENMEMORY flag.
        <li>Mandantory.  Specify 'length' for the size of the memory block in bytes.
        <li>Other flags are optional.
        
        
        <u>Loading a file from within another larger (possibly wad/pak) file, by giving the loader an offset and length.</u>
        <li>Mandantory.  Specify 'fileoffset' and 'length'.
        <li>Other flags are optional.
        
        
        <u>To create a user created / non file based sound.</u>
        <li>Create the sound using the FMOD_OPENUSER flag.
        <li>Mandantory.  Specify 'defaultfrequency, 'numchannels' and 'format'.
        <li>Other flags are optional.
        
        
        <u>To specify a starting subsound to seek to and flush with, within a multi-sample stream (ie FSB/DLS/SF2).</u>
        
        <li>Mandantory.  Specify 'initialsubsound'.
        
        
        <u>To specify which subsounds to load for multi-sample sounds (ie FSB/DLS/SF2) so that memory is saved and only a subset is actually loaded/read from disk.</u>
        
        <li>Mandantory.  Specify 'inclusionlist' and 'inclusionlistnum'.
        
        
        <u>To specify 'piggyback' read and seek callbacks for capture of sound data as fmod reads and decodes it.  Useful for ripping decoded PCM data from sounds as they are loaded / played.</u>
        
        <li>Mandantory.  Specify 'pcmreadcallback' and 'pcmseekcallback'.
        
        
        <u>To specify a MIDI DLS/SF2 sample set file to load when opening a MIDI file.</u>
        
        <li>Mandantory.  Specify 'dlsname'.
        
        
        Setting the 'decodebuffersize' is for cpu intensive codecs that may be causing stuttering, not file intensive codecs (ie those from CD or netstreams) which are normally altered with System::setStreamBufferSize.  As an example of cpu intensive codecs, an mp3 file will take more cpu to decode than a PCM wav file.
        If you have a stuttering effect, then it is using more cpu than the decode buffer playback rate can keep up with.  Increasing the decode buffersize will most likely solve this problem.

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable

        [SEE_ALSO]
        System::createSound
        System::setStreamBufferSize
        FMOD_MODE
    ]
    */
    public struct CREATESOUNDEXINFO
    {
        public int cbsize;                 /* [in] Size of this structure.  This is used so the structure can be expanded in the future and still work on older versions of FMOD Ex. */
        public uint length;                 /* [in] Optional. Specify 0 to ignore. Size in bytes of file to load, or sound to create (in this case only if FMOD_OPENUSER is used).  Required if loading from memory.  If 0 is specified, then it will use the size of the file (unless loading from memory then an error will be returned). */
        public uint fileoffset;             /* [in] Optional. Specify 0 to ignore. Offset from start of the file to start loading from.  This is useful for loading files from inside big data files. */
        public int numchannels;            /* [in] Optional. Specify 0 to ignore. Number of channels in a sound specified only if OPENUSER is used. */
        public int defaultfrequency;       /* [in] Optional. Specify 0 to ignore. Default frequency of sound in a sound specified only if OPENUSER is used.  Other formats use the frequency determined by the file format. */
        public SOUND_FORMAT format;                 /* [in] Optional. Specify 0 or SOUND_FORMAT_NONE to ignore. Format of the sound specified only if OPENUSER is used.  Other formats use the format determined by the file format.   */
        public uint decodebuffersize;       /* [in] Optional. Specify 0 to ignore. For streams.  This determines the size of the double buffer (in PCM samples) that a stream uses.  Use this for user created streams if you want to determine the size of the callback buffer passed to you.  Specify 0 to use FMOD's default size which is currently equivalent to 400ms of the sound format created/loaded. */
        public int initialsubsound;        /* [in] Optional. Specify 0 to ignore. In a multi-sample file format such as .FSB/.DLS/.SF2, specify the initial subsound to seek to, only if CREATESTREAM is used. */
        public int numsubsounds;           /* [in] Optional. Specify 0 to ignore or have no subsounds.  In a user created multi-sample sound, specify the number of subsounds within the sound that are accessable with Sound::getSubSound / SoundGetSubSound. */
        public IntPtr inclusionlist;          /* [in] Optional. Specify 0 to ignore. In a multi-sample format such as .FSB/.DLS/.SF2 it may be desirable to specify only a subset of sounds to be loaded out of the whole file.  This is an array of subsound indicies to load into memory when created. */
        public int inclusionlistnum;       /* [in] Optional. Specify 0 to ignore. This is the number of integers contained within the */
        public SOUND_PCMREADCALLBACK pcmreadcallback;        /* [in] Optional. Specify 0 to ignore. Callback to 'piggyback' on FMOD's read functions and accept or even write PCM data while FMOD is opening the sound.  Used for user sounds created with OPENUSER or for capturing decoded data as FMOD reads it. */
        public SOUND_PCMSETPOSCALLBACK pcmsetposcallback;      /* [in] Optional. Specify 0 to ignore. Callback for when the user calls a seeking function such as Channel::setPosition within a multi-sample sound, and for when it is opened.*/
        public SOUND_NONBLOCKCALLBACK nonblockcallback;       /* [in] Optional. Specify 0 to ignore. Callback for successful completion, or error while loading a sound that used the FMOD_NONBLOCKING flag.*/
        public string dlsname;                /* [in] Optional. Specify 0 to ignore. Filename for a DLS or SF2 sample set when loading a MIDI file.   If not specified, on windows it will attempt to open /windows/system32/drivers/gm.dls, otherwise the MIDI will fail to open.  */
        public string encryptionkey;          /* [in] Optional. Specify 0 to ignore. Key for encrypted FSB file.  Without this key an encrypted FSB file will not load. */
        public int maxpolyphony;           /* [in] Optional. Specify 0 to ingore. For sequenced formats with dynamic channel allocation such as .MID and .IT, this specifies the maximum voice count allowed while playing.  .IT defaults to 64.  .MID defaults to 32. */
        public IntPtr userdata;               /* [in] Optional. Specify 0 to ignore. This is user data to be attached to the sound during creation.  Access via Sound::getUserData. */
        public SOUND_TYPE suggestedsoundtype;     /* [in] Optional. Specify 0 or FMOD_SOUND_TYPE_UNKNOWN to ignore.  Instead of scanning all codec types, use this to speed up loading by making it jump straight to this codec. */
    }


    /*
    [STRUCTURE] 
    [
        [DESCRIPTION]
        Structure defining a reverb environment.
        
        For more indepth descriptions of the reverb properties under win32, please see the EAX2 and EAX3
        documentation at http://developer.creative.com/ under the 'downloads' section.
        If they do not have the EAX3 documentation, then most information can be attained from
        the EAX2 documentation, as EAX3 only adds some more parameters and functionality on top of 
        EAX2.

        [REMARKS]
        Note the default reverb properties are the same as the FMOD_PRESET_GENERIC preset.
        Note that integer values that typically range from -10,000 to 1000 are represented in 
        decibels, and are of a logarithmic scale, not linear, wheras float values are always linear.
        PORTABILITY: Each member has the platform it supports in braces ie (win32/xbox).  
        Some reverb parameters are only supported in win32 and some only on xbox. If all parameters are set then
        the reverb should product a similar effect on either platform.
        Win32/Win64 - This is only supported with FMOD_OUTPUTTYPE_DSOUND and EAX compatible sound cards. 
        Macintosh - Currently unsupported. 
        Linux - Currently unsupported. 
        XBox - Only a subset of parameters are supported.  
        PlayStation 2 - Only the Environment and Flags paramenters are supported. 
        GameCube - Only a subset of parameters are supported. 
        
        The numerical values listed below are the maximum, minimum and default values for each variable respectively.
        
        Members marked with [in] mean the user sets the value before passing it to the function.
        Members marked with [out] mean FMOD sets the value to be used after the function exits.

        [PLATFORMS]
        Win32, Win64, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable

        [SEE_ALSO]
        System::setReverbProperties
        System::getReverbProperties
        REVERB_PRESETS
        REVERB_FLAGS
    ]
    */
    public class REVERB_PROPERTIES
    {                                   /*          MIN     MAX    DEFAULT   DESCRIPTION */
        public int Instance;          /* [in]     0     , 2     , 0      , EAX4 only. Environment Instance. 3 seperate reverbs simultaneously are possible. This specifies which one to set. (win32 only) */
        public uint Environment;       /* [in/out] 0     , 25    , 0      , sets all listener properties (win32/ps2) */
        public float EnvSize;           /* [in/out] 1.0   , 100.0 , 7.5    , environment size in meters (win32 only) */
        public float EnvDiffusion;      /* [in/out] 0.0   , 1.0   , 1.0    , environment diffusion (win32/xbox) */
        public int Room;              /* [in/out] -10000, 0     , -1000  , room effect level (at mid frequencies) (win32/xbox) */
        public int RoomHF;            /* [in/out] -10000, 0     , -100   , relative room effect level at high frequencies (win32/xbox) */
        public int RoomLF;            /* [in/out] -10000, 0     , 0      , relative room effect level at low frequencies (win32 only) */
        public float DecayTime;         /* [in/out] 0.1   , 20.0  , 1.49   , reverberation decay time at mid frequencies (win32/xbox) */
        public float DecayHFRatio;      /* [in/out] 0.1   , 2.0   , 0.83   , high-frequency to mid-frequency decay time ratio (win32/xbox) */
        public float DecayLFRatio;      /* [in/out] 0.1   , 2.0   , 1.0    , low-frequency to mid-frequency decay time ratio (win32 only) */
        public int Reflections;       /* [in/out] -10000, 1000  , -2602  , early reflections level relative to room effect (win32/xbox) */
        public float ReflectionsDelay;  /* [in/out] 0.0   , 0.3   , 0.007  , initial reflection delay time (win32/xbox) */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] ReflectionsPan;  /* [in/out]       ,       , [0,0,0], early reflections panning vector (win32 only) */
        public int Reverb;            /* [in/out] -10000, 2000  , 200    , late reverberation level relative to room effect (win32/xbox) */
        public float ReverbDelay;       /* [in/out] 0.0   , 0.1   , 0.011  , late reverberation delay time relative to initial reflection (win32/xbox) */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] ReverbPan;       /* [in/out]       ,       , [0,0,0], late reverberation panning vector (win32 only) */
        public float EchoTime;          /* [in/out] .075  , 0.25  , 0.25   , echo time (win32 only) */
        public float EchoDepth;         /* [in/out] 0.0   , 1.0   , 0.0    , echo depth (win32 only) */
        public float ModulationTime;    /* [in/out] 0.04  , 4.0   , 0.25   , modulation time (win32 only) */
        public float ModulationDepth;   /* [in/out] 0.0   , 1.0   , 0.0    , modulation depth (win32 only) */
        public float AirAbsorptionHF;   /* [in/out] -100  , 0.0   , -5.0   , change in level per meter at high frequencies (win32 only) */
        public float HFReference;       /* [in/out] 1000.0, 20000 , 5000.0 , reference high frequency (hz) (win32/xbox) */
        public float LFReference;       /* [in/out] 20.0  , 1000.0, 250.0  , reference low frequency (hz) (win32 only) */
        public float RoomRolloffFactor; /* [in/out] 0.0   , 10.0  , 0.0    , like rolloffscale in System::set3DSettings but for reverb room size effect (win32/Xbox) */
        public float Diffusion;         /* [in/out] 0.0   , 100.0 , 100.0  , Value that controls the echo density in the late reverberation decay. (xbox only) */
        public float Density;           /* [in/out] 0.0   , 100.0 , 100.0  , Value that controls the modal density in the late reverberation decay (xbox only) */
        public uint Flags;             /* [in/out] REVERB_FLAGS - modifies the behavior of above properties (win32/ps2) */

        #region wrapperinternal
        public REVERB_PROPERTIES(int instance, uint environment, float envSize, float envDiffusion, int room, int roomHF, int roomLF,
                          float decayTime, float decayHFRatio, float decayLFRatio, int reflections, float reflectionsDelay,
                          float reflectionsPanx, float reflectionsPany, float reflectionsPanz, int reverb, float reverbDelay,
                          float reverbPanx, float reverbPany, float reverbPanz, float echoTime, float echoDepth, float modulationTime,
                          float modulationDepth, float airAbsorptionHF, float hfReference, float lfReference, float roomRolloffFactor,
                          float diffusion, float density, uint flags)
        {
            Instance = instance;
            Environment = environment;
            EnvSize = envSize;
            EnvDiffusion = envDiffusion;
            Room = room;
            RoomHF = roomHF;
            RoomLF = roomLF;
            DecayTime = decayTime;
            DecayHFRatio = decayHFRatio;
            DecayLFRatio = decayLFRatio;
            Reflections = reflections;
            ReflectionsDelay = reflectionsDelay;
            ReflectionsPan[0] = reflectionsPanx;
            ReflectionsPan[1] = reflectionsPany;
            ReflectionsPan[2] = reflectionsPanz;
            Reverb = reverb;
            ReverbDelay = reverbDelay;
            ReverbPan[0] = reverbPanx;
            ReverbPan[1] = reverbPany;
            ReverbPan[2] = reverbPanz;
            EchoTime = echoTime;
            EchoDepth = echoDepth;
            ModulationTime = modulationTime;
            ModulationDepth = modulationDepth;
            AirAbsorptionHF = airAbsorptionHF;
            HFReference = hfReference;
            LFReference = lfReference;
            RoomRolloffFactor = roomRolloffFactor;
            Diffusion = diffusion;
            Density = density;
            Flags = flags;
        }
        #endregion
    }


    /*
    [DEFINE] 
    [
        [NAME] 
        REVERB_FLAGS

        [DESCRIPTION]
        Values for the Flags member of the REVERB_PROPERTIES structure.

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable

        [SEE_ALSO]
        REVERB_PROPERTIES
    ]
    */
    public class REVERB_FLAGS
    {
        public const uint DECAYTIMESCALE = 0x00000001;   /* 'EnvSize' affects reverberation decay time */
        public const uint REFLECTIONSSCALE = 0x00000002;   /* 'EnvSize' affects reflection level */
        public const uint REFLECTIONSDELAYSCALE = 0x00000004;   /* 'EnvSize' affects initial reflection delay time */
        public const uint REVERBSCALE = 0x00000008;   /* 'EnvSize' affects reflections level */
        public const uint REVERBDELAYSCALE = 0x00000010;   /* 'EnvSize' affects late reverberation delay time */
        public const uint DECAYHFLIMIT = 0x00000020;   /* AirAbsorptionHF affects DecayHFRatio */
        public const uint ECHOTIMESCALE = 0x00000040;   /* 'EnvSize' affects echo time */
        public const uint MODULATIONTIMESCALE = 0x00000080;   /* 'EnvSize' affects modulation time */
        public const uint DEFAULT = DECAYTIMESCALE |
            REFLECTIONSSCALE |
            REFLECTIONSDELAYSCALE |
            REVERBSCALE |
            REVERBDELAYSCALE |
            DECAYHFLIMIT;
    }


    /*
    [DEFINE] 
    [
    [NAME] 
    FMOD_REVERB_PRESETS

    [DESCRIPTION]   
    A set of predefined environment PARAMETERS, created by Creative Labs
    These are used to initialize an FMOD_REVERB_PROPERTIES structure statically.
    ie 
    FMOD_REVERB_PROPERTIES prop = FMOD_PRESET_GENERIC;

    [PLATFORMS]
    Win32, Win64, Linux, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable

    [SEE_ALSO]
    System::setReverbProperties
    ]
    */
    class PRESET
    {
        /*                                                                           Instance  Env   Size    Diffus  Room   RoomHF  RmLF DecTm   DecHF  DecLF   Refl  RefDel  RefPan           Revb  RevDel  ReverbPan       EchoTm  EchDp  ModTm  ModDp  AirAbs  HFRef    LFRef  RRlOff Diffus  Densty  FLAGS */
        public REVERB_PROPERTIES OFF() { return new REVERB_PROPERTIES(0, 0, 7.5f, 1.00f, -10000, -10000, 0, 1.00f, 1.00f, 1.0f, -2602, 0.007f, 0.0f, 0.0f, 0.0f, 200, 0.011f, 0.0f, 0.0f, 0.0f, 0.250f, 0.00f, 0.25f, 0.000f, -5.0f, 5000.0f, 250.0f, 0.0f, 0.0f, 0.0f, 0x3f); }
        public REVERB_PROPERTIES GENERIC() { return new REVERB_PROPERTIES(0, 0, 7.5f, 1.00f, -1000, -100, 0, 1.49f, 0.83f, 1.0f, -2602, 0.007f, 0.0f, 0.0f, 0.0f, 200, 0.011f, 0.0f, 0.0f, 0.0f, 0.250f, 0.00f, 0.25f, 0.000f, -5.0f, 5000.0f, 250.0f, 0.0f, 100.0f, 100.0f, 0x3f); }
        public REVERB_PROPERTIES PADDEDCELL() { return new REVERB_PROPERTIES(0, 1, 1.4f, 1.00f, -1000, -6000, 0, 0.17f, 0.10f, 1.0f, -1204, 0.001f, 0.0f, 0.0f, 0.0f, 207, 0.002f, 0.0f, 0.0f, 0.0f, 0.250f, 0.00f, 0.25f, 0.000f, -5.0f, 5000.0f, 250.0f, 0.0f, 100.0f, 100.0f, 0x3f); }
        public REVERB_PROPERTIES ROOM() { return new REVERB_PROPERTIES(0, 2, 1.9f, 1.00f, -1000, -454, 0, 0.40f, 0.83f, 1.0f, -1646, 0.002f, 0.0f, 0.0f, 0.0f, 53, 0.003f, 0.0f, 0.0f, 0.0f, 0.250f, 0.00f, 0.25f, 0.000f, -5.0f, 5000.0f, 250.0f, 0.0f, 100.0f, 100.0f, 0x3f); }
        public REVERB_PROPERTIES BATHROOM() { return new REVERB_PROPERTIES(0, 3, 1.4f, 1.00f, -1000, -1200, 0, 1.49f, 0.54f, 1.0f, -370, 0.007f, 0.0f, 0.0f, 0.0f, 1030, 0.011f, 0.0f, 0.0f, 0.0f, 0.250f, 0.00f, 0.25f, 0.000f, -5.0f, 5000.0f, 250.0f, 0.0f, 100.0f, 60.0f, 0x3f); }
        public REVERB_PROPERTIES LIVINGROOM() { return new REVERB_PROPERTIES(0, 4, 2.5f, 1.00f, -1000, -6000, 0, 0.50f, 0.10f, 1.0f, -1376, 0.003f, 0.0f, 0.0f, 0.0f, -1104, 0.004f, 0.0f, 0.0f, 0.0f, 0.250f, 0.00f, 0.25f, 0.000f, -5.0f, 5000.0f, 250.0f, 0.0f, 100.0f, 100.0f, 0x3f); }
        public REVERB_PROPERTIES STONEROOM() { return new REVERB_PROPERTIES(0, 5, 11.6f, 1.00f, -1000, -300, 0, 2.31f, 0.64f, 1.0f, -711, 0.012f, 0.0f, 0.0f, 0.0f, 83, 0.017f, 0.0f, 0.0f, 0.0f, 0.250f, 0.00f, 0.25f, 0.000f, -5.0f, 5000.0f, 250.0f, 0.0f, 100.0f, 100.0f, 0x3f); }
        public REVERB_PROPERTIES AUDITORIUM() { return new REVERB_PROPERTIES(0, 6, 21.6f, 1.00f, -1000, -476, 0, 4.32f, 0.59f, 1.0f, -789, 0.020f, 0.0f, 0.0f, 0.0f, -289, 0.030f, 0.0f, 0.0f, 0.0f, 0.250f, 0.00f, 0.25f, 0.000f, -5.0f, 5000.0f, 250.0f, 0.0f, 100.0f, 100.0f, 0x3f); }
        public REVERB_PROPERTIES CONCERTHALL() { return new REVERB_PROPERTIES(0, 7, 19.6f, 1.00f, -1000, -500, 0, 3.92f, 0.70f, 1.0f, -1230, 0.020f, 0.0f, 0.0f, 0.0f, -2, 0.029f, 0.0f, 0.0f, 0.0f, 0.250f, 0.00f, 0.25f, 0.000f, -5.0f, 5000.0f, 250.0f, 0.0f, 100.0f, 100.0f, 0x3f); }
        public REVERB_PROPERTIES CAVE() { return new REVERB_PROPERTIES(0, 8, 14.6f, 1.00f, -1000, 0, 0, 2.91f, 1.30f, 1.0f, -602, 0.015f, 0.0f, 0.0f, 0.0f, -302, 0.022f, 0.0f, 0.0f, 0.0f, 0.250f, 0.00f, 0.25f, 0.000f, -5.0f, 5000.0f, 250.0f, 0.0f, 100.0f, 100.0f, 0x1f); }
        public REVERB_PROPERTIES ARENA() { return new REVERB_PROPERTIES(0, 9, 36.2f, 1.00f, -1000, -698, 0, 7.24f, 0.33f, 1.0f, -1166, 0.020f, 0.0f, 0.0f, 0.0f, 16, 0.030f, 0.0f, 0.0f, 0.0f, 0.250f, 0.00f, 0.25f, 0.000f, -5.0f, 5000.0f, 250.0f, 0.0f, 100.0f, 100.0f, 0x3f); }
        public REVERB_PROPERTIES HANGAR() { return new REVERB_PROPERTIES(0, 10, 50.3f, 1.00f, -1000, -1000, 0, 10.05f, 0.23f, 1.0f, -602, 0.020f, 0.0f, 0.0f, 0.0f, 198, 0.030f, 0.0f, 0.0f, 0.0f, 0.250f, 0.00f, 0.25f, 0.000f, -5.0f, 5000.0f, 250.0f, 0.0f, 100.0f, 100.0f, 0x3f); }
        public REVERB_PROPERTIES CARPETTEDHALLWAY() { return new REVERB_PROPERTIES(0, 11, 1.9f, 1.00f, -1000, -4000, 0, 0.30f, 0.10f, 1.0f, -1831, 0.002f, 0.0f, 0.0f, 0.0f, -1630, 0.030f, 0.0f, 0.0f, 0.0f, 0.250f, 0.00f, 0.25f, 0.000f, -5.0f, 5000.0f, 250.0f, 0.0f, 100.0f, 100.0f, 0x3f); }
        public REVERB_PROPERTIES HALLWAY() { return new REVERB_PROPERTIES(0, 12, 1.8f, 1.00f, -1000, -300, 0, 1.49f, 0.59f, 1.0f, -1219, 0.007f, 0.0f, 0.0f, 0.0f, 441, 0.011f, 0.0f, 0.0f, 0.0f, 0.250f, 0.00f, 0.25f, 0.000f, -5.0f, 5000.0f, 250.0f, 0.0f, 100.0f, 100.0f, 0x3f); }
        public REVERB_PROPERTIES STONECORRIDOR() { return new REVERB_PROPERTIES(0, 13, 13.5f, 1.00f, -1000, -237, 0, 2.70f, 0.79f, 1.0f, -1214, 0.013f, 0.0f, 0.0f, 0.0f, 395, 0.020f, 0.0f, 0.0f, 0.0f, 0.250f, 0.00f, 0.25f, 0.000f, -5.0f, 5000.0f, 250.0f, 0.0f, 100.0f, 100.0f, 0x3f); }
        public REVERB_PROPERTIES ALLEY() { return new REVERB_PROPERTIES(0, 14, 7.5f, 0.30f, -1000, -270, 0, 1.49f, 0.86f, 1.0f, -1204, 0.007f, 0.0f, 0.0f, 0.0f, -4, 0.011f, 0.0f, 0.0f, 0.0f, 0.125f, 0.95f, 0.25f, 0.000f, -5.0f, 5000.0f, 250.0f, 0.0f, 100.0f, 100.0f, 0x3f); }
        public REVERB_PROPERTIES FOREST() { return new REVERB_PROPERTIES(0, 15, 38.0f, 0.30f, -1000, -3300, 0, 1.49f, 0.54f, 1.0f, -2560, 0.162f, 0.0f, 0.0f, 0.0f, -229, 0.088f, 0.0f, 0.0f, 0.0f, 0.125f, 1.00f, 0.25f, 0.000f, -5.0f, 5000.0f, 250.0f, 0.0f, 79.0f, 100.0f, 0x3f); }
        public REVERB_PROPERTIES CITY() { return new REVERB_PROPERTIES(0, 16, 7.5f, 0.50f, -1000, -800, 0, 1.49f, 0.67f, 1.0f, -2273, 0.007f, 0.0f, 0.0f, 0.0f, -1691, 0.011f, 0.0f, 0.0f, 0.0f, 0.250f, 0.00f, 0.25f, 0.000f, -5.0f, 5000.0f, 250.0f, 0.0f, 50.0f, 100.0f, 0x3f); }
        public REVERB_PROPERTIES MOUNTAINS() { return new REVERB_PROPERTIES(0, 17, 100.0f, 0.27f, -1000, -2500, 0, 1.49f, 0.21f, 1.0f, -2780, 0.300f, 0.0f, 0.0f, 0.0f, -1434, 0.100f, 0.0f, 0.0f, 0.0f, 0.250f, 1.00f, 0.25f, 0.000f, -5.0f, 5000.0f, 250.0f, 0.0f, 27.0f, 100.0f, 0x1f); }
        public REVERB_PROPERTIES QUARRY() { return new REVERB_PROPERTIES(0, 18, 17.5f, 1.00f, -1000, -1000, 0, 1.49f, 0.83f, 1.0f, -10000, 0.061f, 0.0f, 0.0f, 0.0f, 500, 0.025f, 0.0f, 0.0f, 0.0f, 0.125f, 0.70f, 0.25f, 0.000f, -5.0f, 5000.0f, 250.0f, 0.0f, 100.0f, 100.0f, 0x3f); }
        public REVERB_PROPERTIES PLAIN() { return new REVERB_PROPERTIES(0, 19, 42.5f, 0.21f, -1000, -2000, 0, 1.49f, 0.50f, 1.0f, -2466, 0.179f, 0.0f, 0.0f, 0.0f, -1926, 0.100f, 0.0f, 0.0f, 0.0f, 0.250f, 1.00f, 0.25f, 0.000f, -5.0f, 5000.0f, 250.0f, 0.0f, 21.0f, 100.0f, 0x3f); }
        public REVERB_PROPERTIES PARKINGLOT() { return new REVERB_PROPERTIES(0, 20, 8.3f, 1.00f, -1000, 0, 0, 1.65f, 1.50f, 1.0f, -1363, 0.008f, 0.0f, 0.0f, 0.0f, -1153, 0.012f, 0.0f, 0.0f, 0.0f, 0.250f, 0.00f, 0.25f, 0.000f, -5.0f, 5000.0f, 250.0f, 0.0f, 100.0f, 100.0f, 0x1f); }
        public REVERB_PROPERTIES SEWERPIPE() { return new REVERB_PROPERTIES(0, 21, 1.7f, 0.80f, -1000, -1000, 0, 2.81f, 0.14f, 1.0f, 429, 0.014f, 0.0f, 0.0f, 0.0f, 1023, 0.021f, 0.0f, 0.0f, 0.0f, 0.250f, 0.00f, 0.25f, 0.000f, -5.0f, 5000.0f, 250.0f, 0.0f, 80.0f, 60.0f, 0x3f); }
        public REVERB_PROPERTIES UNDERWATER() { return new REVERB_PROPERTIES(0, 22, 1.8f, 1.00f, -1000, -4000, 0, 1.49f, 0.10f, 1.0f, -449, 0.007f, 0.0f, 0.0f, 0.0f, 1700, 0.011f, 0.0f, 0.0f, 0.0f, 0.250f, 0.00f, 1.18f, 0.348f, -5.0f, 5000.0f, 250.0f, 0.0f, 100.0f, 100.0f, 0x3f); }

        /* Non I3DL2 presets */

        public REVERB_PROPERTIES DRUGGED() { return new REVERB_PROPERTIES(0, 23, 1.9f, 0.50f, -1000, 0, 0, 8.39f, 1.39f, 1.0f, -115, 0.002f, 0.0f, 0.0f, 0.0f, 985, 0.030f, 0.0f, 0.0f, 0.0f, 0.250f, 0.00f, 0.25f, 1.000f, -5.0f, 5000.0f, 250.0f, 0.0f, 100.0f, 100.0f, 0x1f); }
        public REVERB_PROPERTIES DIZZY() { return new REVERB_PROPERTIES(0, 24, 1.8f, 0.60f, -1000, -400, 0, 17.23f, 0.56f, 1.0f, -1713, 0.020f, 0.0f, 0.0f, 0.0f, -613, 0.030f, 0.0f, 0.0f, 0.0f, 0.250f, 1.00f, 0.81f, 0.310f, -5.0f, 5000.0f, 250.0f, 0.0f, 100.0f, 100.0f, 0x1f); }
        public REVERB_PROPERTIES PSYCHOTIC() { return new REVERB_PROPERTIES(0, 25, 1.0f, 0.50f, -1000, -151, 0, 7.56f, 0.91f, 1.0f, -626, 0.020f, 0.0f, 0.0f, 0.0f, 774, 0.030f, 0.0f, 0.0f, 0.0f, 0.250f, 0.00f, 4.00f, 1.000f, -5.0f, 5000.0f, 250.0f, 0.0f, 100.0f, 100.0f, 0x1f); }

        /* PlayStation 2 Only presets */

        public REVERB_PROPERTIES PS2_ROOM() { return new REVERB_PROPERTIES(0, 1, 0, 0, 0, 0, 0, 0.0f, 0.0f, 0.0f, 0, 0.000f, 0.0f, 0.0f, 0.0f, 0, 0.000f, 0.0f, 0.0f, 0.0f, 0.000f, 0.00f, 0.00f, 0.000f, 0.0f, 0000.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0x31f); }
        public REVERB_PROPERTIES PS2_STUDIO_A() { return new REVERB_PROPERTIES(0, 2, 0, 0, 0, 0, 0, 0.0f, 0.0f, 0.0f, 0, 0.000f, 0.0f, 0.0f, 0.0f, 0, 0.000f, 0.0f, 0.0f, 0.0f, 0.000f, 0.00f, 0.00f, 0.000f, 0.0f, 0000.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0x31f); }
        public REVERB_PROPERTIES PS2_STUDIO_B() { return new REVERB_PROPERTIES(0, 3, 0, 0, 0, 0, 0, 0.0f, 0.0f, 0.0f, 0, 0.000f, 0.0f, 0.0f, 0.0f, 0, 0.000f, 0.0f, 0.0f, 0.0f, 0.000f, 0.00f, 0.00f, 0.000f, 0.0f, 0000.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0x31f); }
        public REVERB_PROPERTIES PS2_STUDIO_C() { return new REVERB_PROPERTIES(0, 4, 0, 0, 0, 0, 0, 0.0f, 0.0f, 0.0f, 0, 0.000f, 0.0f, 0.0f, 0.0f, 0, 0.000f, 0.0f, 0.0f, 0.0f, 0.000f, 0.00f, 0.00f, 0.000f, 0.0f, 0000.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0x31f); }
        public REVERB_PROPERTIES PS2_HALL() { return new REVERB_PROPERTIES(0, 5, 0, 0, 0, 0, 0, 0.0f, 0.0f, 0.0f, 0, 0.000f, 0.0f, 0.0f, 0.0f, 0, 0.000f, 0.0f, 0.0f, 0.0f, 0.000f, 0.00f, 0.00f, 0.000f, 0.0f, 0000.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0x31f); }
        public REVERB_PROPERTIES PS2_SPACE() { return new REVERB_PROPERTIES(0, 6, 0, 0, 0, 0, 0, 0.0f, 0.0f, 0.0f, 0, 0.000f, 0.0f, 0.0f, 0.0f, 0, 0.000f, 0.0f, 0.0f, 0.0f, 0.000f, 0.00f, 0.00f, 0.000f, 0.0f, 0000.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0x31f); }
        public REVERB_PROPERTIES PS2_ECHO() { return new REVERB_PROPERTIES(0, 7, 0, 0, 0, 0, 0, 0.0f, 0.0f, 0.0f, 0, 0.000f, 0.0f, 0.0f, 0.0f, 0, 0.000f, 0.0f, 0.0f, 0.0f, 0.000f, 0.00f, 0.00f, 0.000f, 0.0f, 0000.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0x31f); }
        public REVERB_PROPERTIES PS2_DELAY() { return new REVERB_PROPERTIES(0, 8, 0, 0, 0, 0, 0, 0.0f, 0.0f, 0.0f, 0, 0.000f, 0.0f, 0.0f, 0.0f, 0, 0.000f, 0.0f, 0.0f, 0.0f, 0.000f, 0.00f, 0.00f, 0.000f, 0.0f, 0000.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0x31f); }
        public REVERB_PROPERTIES PS2_PIPE() { return new REVERB_PROPERTIES(0, 9, 0, 0, 0, 0, 0, 0.0f, 0.0f, 0.0f, 0, 0.000f, 0.0f, 0.0f, 0.0f, 0, 0.000f, 0.0f, 0.0f, 0.0f, 0.000f, 0.00f, 0.00f, 0.000f, 0.0f, 0000.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0x31f); }
    }

    /*
    [STRUCTURE] 
    [
        [DESCRIPTION]
        Structure defining the properties for a reverb source, related to a FMOD channel.

        For more indepth descriptions of the reverb properties under win32, please see the EAX3
        documentation at http://developer.creative.com/ under the 'downloads' section.
        If they do not have the EAX3 documentation, then most information can be attained from
        the EAX2 documentation, as EAX3 only adds some more parameters and functionality on top of 
        EAX2.

        Note the default reverb properties are the same as the PRESET_GENERIC preset.
        Note that integer values that typically range from -10,000 to 1000 are represented in 
        decibels, and are of a logarithmic scale, not linear, wheras FLOAT values are typically linear.
        PORTABILITY: Each member has the platform it supports in braces ie (win32/xbox).  
        Some reverb parameters are only supported in win32 and some only on xbox. If all parameters are set then
        the reverb should product a similar effect on either platform.
        Linux and FMODCE do not support the reverb api.

        The numerical values listed below are the maximum, minimum and default values for each variable respectively.

        [REMARKS]
        For EAX4 support with multiple reverb environments, set FMOD_REVERB_CHANNELFLAGS_ENVIRONMENT0,
        FMOD_REVERB_CHANNELFLAGS_ENVIRONMENT1 or/and FMOD_REVERB_CHANNELFLAGS_ENVIRONMENT2 in the flags member 
        of FMOD_REVERB_CHANNELPROPERTIES to specify which environment instance(s) to target. 
        Only up to 2 environments to target can be specified at once. Specifying three will result in an error.
        If the sound card does not support EAX4, the environment flag is ignored.

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable

        [SEE_ALSO]
        Channel::setReverbProperties
        Channel::getReverbProperties
        REVERB_CHANNELFLAGS
    ]
    */
    public struct REVERB_CHANNELPROPERTIES
    {                                      /*          MIN     MAX    DEFAULT  DESCRIPTION */
        public int Direct;               /* [in/out] -10000, 1000,  0,       direct path level (at low and mid frequencies) (win32/xbox) */
        public int DirectHF;             /* [in/out] -10000, 0,     0,       relative direct path level at high frequencies (win32/xbox) */
        public int Room;                 /* [in/out] -10000, 1000,  0,       room effect level (at low and mid frequencies) (win32/xbox) */
        public int RoomHF;               /* [in/out] -10000, 0,     0,       relative room effect level at high frequencies (win32/xbox) */
        public int Obstruction;          /* [in/out] -10000, 0,     0,       main obstruction control (attenuation at high frequencies)  (win32/xbox) */
        public float ObstructionLFRatio;   /* [in/out] 0.0,    1.0,   0.0,     obstruction low-frequency level re. main control (win32/xbox) */
        public int Occlusion;            /* [in/out] -10000, 0,     0,       main occlusion control (attenuation at high frequencies) (win32/xbox) */
        public float OcclusionLFRatio;     /* [in/out] 0.0,    1.0,   0.25,    occlusion low-frequency level re. main control (win32/xbox) */
        public float OcclusionRoomRatio;   /* [in/out] 0.0,    10.0,  1.5,     relative occlusion control for room effect (win32) */
        public float OcclusionDirectRatio; /* [in/out] 0.0,    10.0,  1.0,     relative occlusion control for direct path (win32) */
        public int Exclusion;            /* [in/out] -10000, 0,     0,       main exlusion control (attenuation at high frequencies) (win32) */
        public float ExclusionLFRatio;     /* [in/out] 0.0,    1.0,   1.0,     exclusion low-frequency level re. main control (win32) */
        public int OutsideVolumeHF;      /* [in/out] -10000, 0,     0,       outside sound cone level at high frequencies (win32) */
        public float DopplerFactor;        /* [in/out] 0.0,    10.0,  0.0,     like DS3D flDopplerFactor but per source (win32) */
        public float RolloffFactor;        /* [in/out] 0.0,    10.0,  0.0,     like DS3D flRolloffFactor but per source (win32) */
        public float RoomRolloffFactor;    /* [in/out] 0.0,    10.0,  0.0,     like DS3D flRolloffFactor but for room effect (win32/xbox) */
        public float AirAbsorptionFactor;  /* [in/out] 0.0,    10.0,  1.0,     multiplies AirAbsorptionHF member of REVERB_PROPERTIES (win32) */
        public uint Flags;                /* [in/out] REVERB_CHANNELFLAGS - modifies the behavior of properties (win32) */
    }


    /*
    [DEFINE] 
    [
        [NAME] 
        REVERB_CHANNELFLAGS

        [DESCRIPTION]
        Values for the Flags member of the REVERB_CHANNELPROPERTIES structure.

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable

        [SEE_ALSO]
        REVERB_CHANNELPROPERTIES
    ]
    */
    public class REVERB_CHANNELFLAGS
    {
        public const uint DIRECTHFAUTO = 0x00000001; /* Automatic setting of 'Direct'  due to distance from listener */
        public const uint ROOMAUTO = 0x00000002; /* Automatic setting of 'Room'  due to distance from listener */
        public const uint ROOMHFAUTO = 0x00000004; /* Automatic setting of 'RoomHF' due to distance from listener */
        public const uint ENVIRONMENT0 = 0x00000008; /* EAX4 only. Specify channel to target reverb instance 0. */
        public const uint ENVIRONMENT1 = 0x00000010; /* EAX4 only. Specify channel to target reverb instance 1. */
        public const uint ENVIRONMENT2 = 0x00000020; /* EAX4 only. Specify channel to target reverb instance 2. */
        public const uint DEFAULT = DIRECTHFAUTO | ROOMAUTO | ROOMHFAUTO | ENVIRONMENT0;
    }


    /*
    [STRUCTURE] 
    [
        [DESCRIPTION]
        Settings for advanced features like configuring memory and cpu usage for the FMOD_CREATECOMPRESSEDSAMPLE feature.
   
        [REMARKS]
        maxMPEGcodecs / maxADPCMcodecs / maxXMAcodecs will determine the maximum cpu usage of playing realtime samples.  Use this to lower potential excess cpu usage and also control memory usage.<br>
   
        [PLATFORMS]
        Win32, Win64, Linux, Linux64, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable, PlayStation 3
   
        [SEE_ALSO]
        System::setAdvancedSettings
        System::getAdvancedSettings
    ]
    */
    public class ADVANCEDSETTINGS
    {
        public int cbsize;             /* Size of structure.  Use sizeof(FMOD_ADVANCEDSETTINGS) */
        public int maxMPEGcodecs;      /* For use with FMOD_CREATECOMPRESSEDSAMPLE only.  Mpeg  codecs consume 48,696 per instance and this number will determine how many mpeg channels can be played simultaneously.  Default = 16. */
        public int maxADPCMcodecs;     /* For use with FMOD_CREATECOMPRESSEDSAMPLE only.  ADPCM codecs consume 1k per instance and this number will determine how many ADPCM channels can be played simultaneously.  Default = 32. */
        public int maxXMAcodecs;       /* For use with FMOD_CREATECOMPRESSEDSAMPLE only.  XMA   codecs consume 8k per instance and this number will determine how many XMA channels can be played simultaneously.  Default = 32.  */
    }


    /*
    [ENUM] 
    [
        [NAME] 
        FMOD_MISC_VALUES

        [DESCRIPTION]
        Miscellaneous values for FMOD functions.

        [PLATFORMS]
        Win32, Win64, Linux, Macintosh, Xbox, Xbox360, PlayStation 2, GameCube, PlayStation Portable

        [SEE_ALSO]
        System::playSound
        System::playDSP
        System::getChannel
    ]
    */
    public enum CHANNELINDEX
    {
        FREE = -1,     /* For a channel index, FMOD chooses a free voice using the priority system. */
        REUSE = -2      /* For a channel index, re-use the channel handle that was passed in. */
    }


    /* 
        FMOD Callbacks
    */
    public delegate RESULT CHANNEL_CALLBACK(IntPtr channelraw, CHANNEL_CALLBACKTYPE type, int command, uint commanddata1, uint commanddata2);

    public delegate RESULT FILE_OPENCALLBACK(string name, int unicode, ref uint filesize, ref IntPtr handle, ref IntPtr userdata);
    public delegate RESULT FILE_CLOSECALLBACK(IntPtr handle, IntPtr userdata);
    public delegate RESULT FILE_READCALLBACK(IntPtr handle, IntPtr buffer, uint sizebytes, ref uint bytesread, IntPtr userdata);
    public delegate RESULT FILE_SEEKCALLBACK(IntPtr handle, int pos, IntPtr userdata);


    /*
        FMOD System factory functions.  Use this to create an FMOD System Instance.  below you will see System_Init/Close to get started.
    */
    public class Factory
    {
        public static RESULT System_Create(ref System system)
        {
            RESULT result = RESULT.OK;
            IntPtr systemraw = new IntPtr();
            System systemnew = null;

            result = FMOD_System_Create(ref systemraw);
            if (result != RESULT.OK)
            {
                return result;
            }

            systemnew = new System();
            systemnew.setRaw(systemraw);
            system = systemnew;

            return result;
        }


        #region importfunctions

        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_Create(ref IntPtr system);

        #endregion
    }


    /*
        'System' API
    */
    public class System
    {
        public RESULT release()
        {
            return FMOD_System_Release(systemraw);
        }


        // Pre-init functions.
        public RESULT setOutput(OUTPUTTYPE output)
        {
            return FMOD_System_SetOutput(systemraw, output);
        }
        public RESULT getOutput(ref OUTPUTTYPE output)
        {
            return FMOD_System_GetOutput(systemraw, ref output);
        }
        public RESULT getNumDrivers(ref int numdrivers)
        {
            return FMOD_System_GetNumDrivers(systemraw, ref numdrivers);
        }
        public RESULT getDriverName(int id, StringBuilder name, int namelen)
        {
            return FMOD_System_GetDriverName(systemraw, id, name, namelen);
        }
        public RESULT getDriverCaps(int id, ref CAPS caps, ref int minfrequency, ref int maxfrequency, ref SPEAKERMODE controlpanelspeakermode)
        {
            return FMOD_System_GetDriverCaps(systemraw, id, ref caps, ref minfrequency, ref maxfrequency, ref controlpanelspeakermode);
        }
        public RESULT setDriver(int driver)
        {
            return FMOD_System_SetDriver(systemraw, driver);
        }
        public RESULT getDriver(ref int driver)
        {
            return FMOD_System_GetDriver(systemraw, ref driver);
        }
        public RESULT setHardwareChannels(int min2d, int max2d, int min3d, int max3d)
        {
            return FMOD_System_SetHardwareChannels(systemraw, min2d, max2d, min3d, max3d);
        }
        public RESULT getHardwareChannels(ref int num2d, ref int num3d, ref int total)
        {
            return FMOD_System_GetHardwareChannels(systemraw, ref num2d, ref num3d, ref total);
        }
        public RESULT setSoftwareChannels(int numsoftwarechannels)
        {
            return FMOD_System_SetSoftwareChannels(systemraw, numsoftwarechannels);
        }
        public RESULT getSoftwareChannels(ref int numsoftwarechannels)
        {
            return FMOD_System_GetSoftwareChannels(systemraw, ref numsoftwarechannels);
        }
        public RESULT setSoftwareFormat(int samplerate, SOUND_FORMAT format, int numoutputchannels, int maxinputchannels, DSP_RESAMPLER resamplemethod)
        {
            return FMOD_System_SetSoftwareFormat(systemraw, samplerate, format, numoutputchannels, maxinputchannels, resamplemethod);
        }
        public RESULT getSoftwareFormat(ref int samplerate, ref SOUND_FORMAT format, ref int numoutputchannels, ref int maxinputchannels, ref DSP_RESAMPLER resamplemethod, ref int bits)
        {
            return FMOD_System_GetSoftwareFormat(systemraw, ref samplerate, ref format, ref numoutputchannels, ref maxinputchannels, ref resamplemethod, ref bits);
        }
        public RESULT setDSPBufferSize(uint bufferlength, int numbuffers)
        {
            return FMOD_System_SetDSPBufferSize(systemraw, bufferlength, numbuffers);
        }
        public RESULT getDSPBufferSize(ref uint bufferlength, ref int numbuffers)
        {
            return FMOD_System_GetDSPBufferSize(systemraw, ref bufferlength, ref numbuffers);
        }
        public RESULT setFileSystem(FILE_OPENCALLBACK useropen, FILE_CLOSECALLBACK userclose, FILE_READCALLBACK userread, FILE_SEEKCALLBACK userseek, int buffersize)
        {
            return FMOD_System_SetFileSystem(systemraw, useropen, userclose, userread, userseek, buffersize);
        }
        public RESULT attachFileSystem(FILE_OPENCALLBACK useropen, FILE_CLOSECALLBACK userclose, FILE_READCALLBACK userread, FILE_SEEKCALLBACK userseek)
        {
            return FMOD_System_AttachFileSystem(systemraw, useropen, userclose, userread, userseek);
        }
        public RESULT setAdvancedSettings(ref ADVANCEDSETTINGS settings)
        {
            return FMOD_System_SetAdvancedSettings(systemraw, ref settings);
        }
        public RESULT getAdvancedSettings(ref ADVANCEDSETTINGS settings)
        {
            return FMOD_System_GetAdvancedSettings(systemraw, ref settings);
        }
        public RESULT setSpeakerMode(SPEAKERMODE speakermode)
        {
            return FMOD_System_SetSpeakerMode(systemraw, speakermode);
        }
        public RESULT getSpeakerMode(ref SPEAKERMODE speakermode)
        {
            return FMOD_System_GetSpeakerMode(systemraw, ref speakermode);
        }


        // Plug-in support
        public RESULT setPluginPath(string path)
        {
            return FMOD_System_SetPluginPath(systemraw, path);
        }
        public RESULT loadPlugin(string filename, ref PLUGINTYPE plugintype, ref int index)
        {
            return FMOD_System_LoadPlugin(systemraw, filename, ref plugintype, ref index);
        }
        public RESULT getNumPlugins(PLUGINTYPE plugintype, ref int numplugins)
        {
            return FMOD_System_GetNumPlugins(systemraw, plugintype, ref numplugins);
        }
        public RESULT getPluginInfo(PLUGINTYPE plugintype, int index, StringBuilder name, int namelen, ref uint version)
        {
            return FMOD_System_GetPluginInfo(systemraw, plugintype, index, name, namelen, ref version);
        }
        public RESULT unloadPlugin(PLUGINTYPE plugintype, int index)
        {
            return FMOD_System_UnloadPlugin(systemraw, plugintype, index);
        }
        public RESULT setOutputByPlugin(int index)
        {
            return FMOD_System_SetOutputByPlugin(systemraw, index);
        }
        public RESULT getOutputByPlugin(ref int index)
        {
            return FMOD_System_GetOutputByPlugin(systemraw, ref index);
        }


        // Init/Close 
        public RESULT init(int maxchannels, INITFLAG flags, IntPtr extradata)
        {
            return FMOD_System_Init(systemraw, maxchannels, flags, extradata);
        }
        public RESULT close()
        {
            return FMOD_System_Close(systemraw);
        }


        // General post-init system functions
        public RESULT update()
        {
            return FMOD_System_Update(systemraw);
        }
        public RESULT updateFinished()
        {
            return FMOD_System_UpdateFinished(systemraw);
        }

        public RESULT set3DSettings(float dopplerscale, float distancefactor, float rolloffscale)
        {
            return FMOD_System_Set3DSettings(systemraw, dopplerscale, distancefactor, rolloffscale);
        }
        public RESULT get3DSettings(ref float dopplerscale, ref float distancefactor, ref float rolloffscale)
        {
            return FMOD_System_Get3DSettings(systemraw, ref dopplerscale, ref distancefactor, ref rolloffscale);
        }
        public RESULT set3DNumListeners(int numlisteners)
        {
            return FMOD_System_Set3DNumListeners(systemraw, numlisteners);
        }
        public RESULT get3DNumListeners(ref int numlisteners)
        {
            return FMOD_System_Get3DNumListeners(systemraw, ref numlisteners);
        }
        public RESULT set3DListenerAttributes(int listener, ref VECTOR pos, ref VECTOR vel, ref VECTOR forward, ref VECTOR up)
        {
            return FMOD_System_Set3DListenerAttributes(systemraw, listener, ref pos, ref vel, ref forward, ref up);
        }
        public RESULT get3DListenerAttributes(int listener, ref VECTOR pos, ref VECTOR vel, ref VECTOR forward, ref VECTOR up)
        {
            return FMOD_System_Get3DListenerAttributes(systemraw, listener, ref pos, ref vel, ref forward, ref up);
        }

        public RESULT setSpeakerPosition(SPEAKER speaker, float x, float y)
        {
            return FMOD_System_SetSpeakerPosition(systemraw, speaker, x, y);
        }
        public RESULT getSpeakerPosition(SPEAKER speaker, ref float x, ref float y)
        {
            return FMOD_System_GetSpeakerPosition(systemraw, speaker, ref x, ref y);
        }

        public RESULT setStreamBufferSize(uint filebuffersize, TIMEUNIT filebuffersizetype)
        {
            return FMOD_System_SetStreamBufferSize(systemraw, filebuffersize, filebuffersizetype);
        }
        public RESULT getStreamBufferSize(ref uint filebuffersize, ref TIMEUNIT filebuffersizetype)
        {
            return FMOD_System_GetStreamBufferSize(systemraw, ref filebuffersize, ref filebuffersizetype);
        }


        // System information functions.
        public RESULT getVersion(ref uint version)
        {
            return FMOD_System_GetVersion(systemraw, ref version);
        }
        public RESULT getOutputHandle(ref IntPtr handle)
        {
            return FMOD_System_GetOutputHandle(systemraw, ref handle);
        }
        public RESULT getChannelsPlaying(ref int channels)
        {
            return FMOD_System_GetChannelsPlaying(systemraw, ref channels);
        }
        public RESULT getCPUUsage(ref float dsp, ref float stream, ref float update, ref float total)
        {
            return FMOD_System_GetCPUUsage(systemraw, ref dsp, ref stream, ref update, ref total);
        }
        public RESULT getSoundRam(ref int currentalloced, ref int maxalloced, ref int total)
        {
            return FMOD_System_GetSoundRAM(systemraw, ref currentalloced, ref maxalloced, ref total);
        }
        public RESULT getNumCDROMDrives(ref int numdrives)
        {
            return FMOD_System_GetNumCDROMDrives(systemraw, ref numdrives);
        }
        public RESULT getCDROMDriveName(int drive, StringBuilder drivename, int drivenamelen, StringBuilder scsiname, int scsinamelen, StringBuilder devicename, int devicenamelen)
        {
            return FMOD_System_GetCDROMDriveName(systemraw, drive, drivename, drivenamelen, scsiname, scsinamelen, devicename, devicenamelen);
        }
        public RESULT getSpectrum(float[] spectrumarray, int numvalues, int channeloffset, DSP_FFT_WINDOW windowtype)
        {
            return FMOD_System_GetSpectrum(systemraw, spectrumarray, numvalues, channeloffset, windowtype);
        }
        public RESULT getWaveData(float[] wavearray, int numvalues, int channeloffset)
        {
            return FMOD_System_GetWaveData(systemraw, wavearray, numvalues, channeloffset);
        }


        // Sound/DSP/Channel creation and retrieval. 
        public RESULT createSound(string name_or_data, MODE mode, ref CREATESOUNDEXINFO exinfo, ref Sound sound)
        {
            RESULT result = RESULT.OK;
            IntPtr soundraw = new IntPtr();

            try
            {
                result = FMOD_System_CreateSound(systemraw, name_or_data, mode, ref exinfo, ref soundraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (sound == null)
            {
                Sound soundnew = new Sound();
                soundnew.setRaw(soundraw);
                sound = soundnew;
            }
            else
            {
                sound.setRaw(soundraw);
            }

            return result;
        }
        public RESULT createSound(byte[] data, MODE mode, ref CREATESOUNDEXINFO exinfo, ref Sound sound)
        {
            RESULT result = RESULT.OK;
            IntPtr soundraw = new IntPtr();

            try
            {
                result = FMOD_System_CreateSound(systemraw, data, mode, ref exinfo, ref soundraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (sound == null)
            {
                Sound soundnew = new Sound();
                soundnew.setRaw(soundraw);
                sound = soundnew;
            }
            else
            {
                sound.setRaw(soundraw);
            }

            return result;
        }
        public RESULT createSound(string name_or_data, MODE mode, ref Sound sound)
        {
            RESULT result = RESULT.OK;
            IntPtr soundraw = new IntPtr();

            try
            {
                result = FMOD_System_CreateSound(systemraw, name_or_data, mode, 0, ref soundraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (sound == null)
            {
                Sound soundnew = new Sound();
                soundnew.setRaw(soundraw);
                sound = soundnew;
            }
            else
            {
                sound.setRaw(soundraw);
            }

            return result;
        }
        public RESULT createStream(string name_or_data, MODE mode, ref CREATESOUNDEXINFO exinfo, ref Sound sound)
        {
            RESULT result = RESULT.OK;
            IntPtr soundraw = new IntPtr();

            try
            {
                result = FMOD_System_CreateStream(systemraw, name_or_data, mode, ref exinfo, ref soundraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (sound == null)
            {
                Sound soundnew = new Sound();
                soundnew.setRaw(soundraw);
                sound = soundnew;
            }
            else
            {
                sound.setRaw(soundraw);
            }

            return result;
        }
        public RESULT createStream(byte[] data, MODE mode, ref CREATESOUNDEXINFO exinfo, ref Sound sound)
        {
            RESULT result = RESULT.OK;
            IntPtr soundraw = new IntPtr();

            try
            {
                result = FMOD_System_CreateStream(systemraw, data, mode, ref exinfo, ref soundraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (sound == null)
            {
                Sound soundnew = new Sound();
                soundnew.setRaw(soundraw);
                sound = soundnew;
            }
            else
            {
                sound.setRaw(soundraw);
            }

            return result;
        }
        public RESULT createStream(string name_or_data, MODE mode, ref Sound sound)
        {
            RESULT result = RESULT.OK;
            IntPtr soundraw = new IntPtr();

            try
            {
                result = FMOD_System_CreateStream(systemraw, name_or_data, mode, 0, ref soundraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (sound == null)
            {
                Sound soundnew = new Sound();
                soundnew.setRaw(soundraw);
                sound = soundnew;
            }
            else
            {
                sound.setRaw(soundraw);
            }

            return result;
        }
        public RESULT createDSP(ref DSP_DESCRIPTION description, ref DSP dsp)
        {
            RESULT result = RESULT.OK;
            IntPtr dspraw = new IntPtr();

            try
            {
                result = FMOD_System_CreateDSP(systemraw, ref description, ref dspraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (dsp == null)
            {
                DSP dspnew = new DSP();
                dspnew.setRaw(dspraw);
                dsp = dspnew;
            }
            else
            {
                dsp.setRaw(dspraw);
            }

            return result;
        }
        public RESULT createDSPByType(DSP_TYPE type, ref DSP dsp)
        {
            RESULT result = RESULT.OK;
            IntPtr dspraw = new IntPtr();

            try
            {
                result = FMOD_System_CreateDSPByType(systemraw, type, ref dspraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (dsp == null)
            {
                DSP dspnew = new DSP();
                dspnew.setRaw(dspraw);
                dsp = dspnew;
            }
            else
            {
                dsp.setRaw(dspraw);
            }

            return result;
        }
        public RESULT createDSPByIndex(int index, ref DSP dsp)
        {
            RESULT result = RESULT.OK;
            IntPtr dspraw = new IntPtr();

            try
            {
                result = FMOD_System_CreateDSPByIndex(systemraw, index, ref dspraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (dsp == null)
            {
                DSP dspnew = new DSP();
                dspnew.setRaw(dspraw);
                dsp = dspnew;
            }
            else
            {
                dsp.setRaw(dspraw);
            }

            return result;
        }

        public RESULT createChannelGroup(string name, ref ChannelGroup channelgroup)
        {
            RESULT result = RESULT.OK;
            IntPtr channelgroupraw = new IntPtr();

            try
            {
                result = FMOD_System_CreateChannelGroup(systemraw, name, ref channelgroupraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (channelgroup == null)
            {
                ChannelGroup channelgroupnew = new ChannelGroup();
                channelgroupnew.setRaw(channelgroupraw);
                channelgroup = channelgroupnew;
            }
            else
            {
                channelgroup.setRaw(channelgroupraw);
            }

            return result;
        }
        public RESULT playSound(CHANNELINDEX channelid, Sound sound, bool paused, ref Channel channel)
        {
            RESULT result = RESULT.OK;
            IntPtr channelraw = channel != null ? channel.getRaw() : new IntPtr();
            try
            {
                result = FMOD_System_PlaySound(systemraw, channelid, sound.getRaw(), paused, ref channelraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (channel == null)
            {
                Channel channelnew = new Channel();
                channelnew.setRaw(channelraw);
                channel = channelnew;
            }
            else
            {
                channel.setRaw(channelraw);
            }

            return result;
        }
        public RESULT playDSP(CHANNELINDEX channelid, DSP dsp, bool paused, ref Channel channel)
        {
            RESULT result = RESULT.OK;
            IntPtr channelraw = channel != null ? channel.getRaw() : new IntPtr();
            try
            {
                result = FMOD_System_PlayDSP(systemraw, channelid, dsp.getRaw(), paused, ref channelraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (channel == null)
            {
                Channel channelnew = new Channel();
                channelnew.setRaw(channelraw);
                channel = channelnew;
            }
            else
            {
                channel.setRaw(channelraw);
            }

            return result;
        }
        public RESULT getChannel(int channelid, ref Channel channel)
        {
            RESULT result = RESULT.OK;
            IntPtr channelraw = new IntPtr();

            try
            {
                result = FMOD_System_GetChannel(systemraw, channelid, ref channelraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (channel == null)
            {
                Channel channelnew = new Channel();
                channelnew.setRaw(channelraw);
                channel = channelnew;
            }
            else
            {
                channel.setRaw(channelraw);
            }

            return result;
        }

        public RESULT getMasterChannelGroup(ref ChannelGroup channelgroup)
        {
            RESULT result = RESULT.OK;
            IntPtr channelgroupraw = new IntPtr();

            try
            {
                result = FMOD_System_GetMasterChannelGroup(systemraw, ref channelgroupraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (channelgroup == null)
            {
                ChannelGroup channelgroupnew = new ChannelGroup();
                channelgroupnew.setRaw(channelgroupraw);
                channelgroup = channelgroupnew;
            }
            else
            {
                channelgroup.setRaw(channelgroupraw);
            }

            return result;
        }

        // Reverb api
        public RESULT setReverbProperties(ref REVERB_PROPERTIES prop)
        {
            return FMOD_System_SetReverbProperties(systemraw, ref prop);
        }
        public RESULT getReverbProperties(ref REVERB_PROPERTIES prop)
        {
            return FMOD_System_GetReverbProperties(systemraw, ref prop);
        }


        // System level DSP access.
        public RESULT getDSPHead(ref DSP dsp)
        {
            RESULT result = RESULT.OK;
            IntPtr dspraw = new IntPtr();

            try
            {
                result = FMOD_System_GetDSPHead(systemraw, ref dspraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (dsp == null)
            {
                DSP dspnew = new DSP();
                dspnew.setRaw(dspraw);
                dsp = dspnew;
            }
            else
            {
                dsp.setRaw(dspraw);
            }

            return result;
        }
        public RESULT addDSP(DSP dsp)
        {
            return FMOD_System_AddDSP(systemraw, dsp.getRaw());
        }
        public RESULT lockDSP()
        {
            return FMOD_System_LockDSP(systemraw);
        }
        public RESULT unlockDSP()
        {
            return FMOD_System_UnlockDSP(systemraw);
        }


        // Recording api
        public RESULT setRecordDriver(int driver)
        {
            return FMOD_System_SetRecordDriver(systemraw, driver);
        }
        public RESULT getRecordDriver(ref int driver)
        {
            return FMOD_System_GetRecordDriver(systemraw, ref driver);
        }
        public RESULT getRecordNumDrivers(ref int numdrivers)
        {
            return FMOD_System_GetRecordNumDrivers(systemraw, ref numdrivers);
        }
        public RESULT getRecordDriverName(int id, StringBuilder name, int namelen)
        {
            return FMOD_System_GetRecordDriverName(systemraw, id, name, namelen);
        }

        public RESULT getRecordPosition(ref uint position)
        {
            return FMOD_System_GetRecordPosition(systemraw, ref position);
        }
        public RESULT recordStart(Sound sound, bool loop)
        {
            return FMOD_System_RecordStart(systemraw, sound.getRaw(), loop);
        }
        public RESULT recordStop()
        {
            return FMOD_System_RecordStop(systemraw);
        }
        public RESULT isRecording(ref bool recording)
        {
            return FMOD_System_IsRecording(systemraw, ref recording);
        }


        // Geometry api	
        public RESULT createGeometry(int maxpolygons, int maxvertices, ref Geometry geometryf)
        {
            RESULT result = RESULT.OK;
            IntPtr geometryraw = new IntPtr();

            try
            {
                result = FMOD_System_CreateGeometry(systemraw, maxpolygons, maxvertices, ref geometryraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (geometryf == null)
            {
                Geometry geometrynew = new Geometry();
                geometrynew.setRaw(geometryraw);
                geometryf = geometrynew;
            }
            else
            {
                geometryf.setRaw(geometryraw);
            }

            return result;
        }
        public RESULT setGeometrySettings(float maxworldsize)
        {
            return FMOD_System_SetGeometrySettings(systemraw, maxworldsize);
        }
        public RESULT getGeometrySettings(ref float maxworldsize)
        {
            return FMOD_System_GetGeometrySettings(systemraw, ref maxworldsize);
        }
        public RESULT loadGeometry(IntPtr data, int datasize, ref Geometry geometry)
        {
            RESULT result = RESULT.OK;
            IntPtr geometryraw = new IntPtr();

            try
            {
                result = FMOD_System_LoadGeometry(systemraw, data, datasize, ref geometryraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (geometry == null)
            {
                Geometry geometrynew = new Geometry();
                geometrynew.setRaw(geometryraw);
                geometry = geometrynew;
            }
            else
            {
                geometry.setRaw(geometryraw);
            }

            return result;
        }


        // Network functions
        public RESULT setNetworkProxy(string proxy)
        {
            return FMOD_System_SetNetworkProxy(systemraw, proxy);
        }
        public RESULT getProxy(StringBuilder proxy, int proxylen)
        {
            return FMOD_System_GetNetworkProxy(systemraw, proxy, proxylen);
        }
        public RESULT setNetworkTimeout(int timeout)
        {
            return FMOD_System_SetNetworkTimeout(systemraw, timeout);
        }
        public RESULT getNetworkTimeout(ref int timeout)
        {
            return FMOD_System_GetNetworkTimeout(systemraw, ref timeout);
        }

        // Userdata set/get                         
        public RESULT setUserData(IntPtr userdata)
        {
            return FMOD_System_SetUserData(systemraw, userdata);
        }
        public RESULT getUserData(ref IntPtr userdata)
        {
            return FMOD_System_GetUserData(systemraw, ref userdata);
        }



        #region importfunctions

        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_Release(IntPtr system);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_SetOutput(IntPtr system, OUTPUTTYPE output);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetOutput(IntPtr system, ref OUTPUTTYPE output);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetNumDrivers(IntPtr system, ref int numdrivers);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetDriverName(IntPtr system, int id, StringBuilder name, int namelen);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetDriverCaps(IntPtr system, int id, ref CAPS caps, ref int minfrequency, ref int maxfrequency, ref SPEAKERMODE controlpanelspeakermode);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_SetDriver(IntPtr system, int driver);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetDriver(IntPtr system, ref int driver);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_SetHardwareChannels(IntPtr system, int min2d, int max2d, int min3d, int max3d);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetHardwareChannels(IntPtr system, ref int num2d, ref int num3d, ref int total);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_SetSoftwareChannels(IntPtr system, int numsoftwarechannels);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetSoftwareChannels(IntPtr system, ref int numsoftwarechannels);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_SetSoftwareFormat(IntPtr system, int samplerate, SOUND_FORMAT format, int numoutputchannels, int maxinputchannels, DSP_RESAMPLER resamplemethod);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetSoftwareFormat(IntPtr system, ref int samplerate, ref SOUND_FORMAT format, ref int numoutputchannels, ref int maxinputchannels, ref DSP_RESAMPLER resamplemethod, ref int bits);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_SetDSPBufferSize(IntPtr system, uint bufferlength, int numbuffers);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetDSPBufferSize(IntPtr system, ref uint bufferlength, ref int numbuffers);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_SetFileSystem(IntPtr system, FILE_OPENCALLBACK useropen, FILE_CLOSECALLBACK userclose, FILE_READCALLBACK userread, FILE_SEEKCALLBACK userseek, int buffersize);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_AttachFileSystem(IntPtr system, FILE_OPENCALLBACK useropen, FILE_CLOSECALLBACK userclose, FILE_READCALLBACK userread, FILE_SEEKCALLBACK userseek);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_SetPluginPath(IntPtr system, string path);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_LoadPlugin(IntPtr system, string filename, ref PLUGINTYPE plugintype, ref int index);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetNumPlugins(IntPtr system, PLUGINTYPE plugintype, ref int numplugins);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetPluginInfo(IntPtr system, PLUGINTYPE plugintype, int index, StringBuilder name, int namelen, ref uint version);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_UnloadPlugin(IntPtr system, PLUGINTYPE plugintype, int index);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_SetOutputByPlugin(IntPtr system, int index);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetOutputByPlugin(IntPtr system, ref int index);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_Init(IntPtr system, int maxchannels, INITFLAG flags, IntPtr extradata);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_Close(IntPtr system);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_Update(IntPtr system);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_UpdateFinished(IntPtr system);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_SetAdvancedSettings(IntPtr system, ref ADVANCEDSETTINGS settings);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetAdvancedSettings(IntPtr system, ref ADVANCEDSETTINGS settings);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_SetSpeakerMode(IntPtr system, SPEAKERMODE speakermode);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetSpeakerMode(IntPtr system, ref SPEAKERMODE speakermode);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_SetSpeakerPosition(IntPtr system, SPEAKER speaker, float x, float y);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetSpeakerPosition(IntPtr system, SPEAKER speaker, ref float x, ref float y);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_Set3DSettings(IntPtr system, float dopplerscale, float distancefactor, float rolloffscale);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_Get3DSettings(IntPtr system, ref float dopplerscale, ref float distancefactor, ref float rolloffscale);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_Set3DNumListeners(IntPtr system, int numlisteners);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_Get3DNumListeners(IntPtr system, ref int numlisteners);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_Set3DListenerAttributes(IntPtr system, int listener, ref VECTOR pos, ref VECTOR vel, ref VECTOR forward, ref VECTOR up);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_Get3DListenerAttributes(IntPtr system, int listener, ref VECTOR pos, ref VECTOR vel, ref VECTOR forward, ref VECTOR up);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_SetFileBufferSize(IntPtr system, int sizebytes);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetFileBufferSize(IntPtr system, ref int sizebytes);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_SetStreamBufferSize(IntPtr system, uint filebuffersize, TIMEUNIT filebuffersizetype);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetStreamBufferSize(IntPtr system, ref uint filebuffersize, ref TIMEUNIT filebuffersizetype);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetVersion(IntPtr system, ref uint version);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetOutputHandle(IntPtr system, ref IntPtr handle);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetChannelsPlaying(IntPtr system, ref int channels);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetCPUUsage(IntPtr system, ref float dsp, ref float stream, ref float update, ref float total);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetSoundRAM(IntPtr system, ref int currentalloced, ref int maxalloced, ref int total);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetNumCDROMDrives(IntPtr system, ref int numdrives);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetCDROMDriveName(IntPtr system, int drive, StringBuilder drivename, int drivenamelen, StringBuilder scsiname, int scsinamelen, StringBuilder devicename, int devicenamelen);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetSpectrum(IntPtr system, [MarshalAs(UnmanagedType.LPArray)] float[] spectrumarray, int numvalues, int channeloffset, DSP_FFT_WINDOW windowtype);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetWaveData(IntPtr system, [MarshalAs(UnmanagedType.LPArray)] float[] wavearray, int numvalues, int channeloffset);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_CreateSound(IntPtr system, string name_or_data, MODE mode, ref CREATESOUNDEXINFO exinfo, ref IntPtr sound);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_CreateStream(IntPtr system, string name_or_data, MODE mode, ref CREATESOUNDEXINFO exinfo, ref IntPtr sound);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_CreateSound(IntPtr system, string name_or_data, MODE mode, int exinfo, ref IntPtr sound);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_CreateStream(IntPtr system, string name_or_data, MODE mode, int exinfo, ref IntPtr sound);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_CreateSound(IntPtr system, byte[] name_or_data, MODE mode, ref CREATESOUNDEXINFO exinfo, ref IntPtr sound);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_CreateStream(IntPtr system, byte[] name_or_data, MODE mode, ref CREATESOUNDEXINFO exinfo, ref IntPtr sound);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_CreateSound(IntPtr system, byte[] name_or_data, MODE mode, int exinfo, ref IntPtr sound);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_CreateStream(IntPtr system, byte[] name_or_data, MODE mode, int exinfo, ref IntPtr sound);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_CreateDSP(IntPtr system, ref DSP_DESCRIPTION description, ref IntPtr dsp);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_CreateDSPByType(IntPtr system, DSP_TYPE type, ref IntPtr dsp);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_CreateDSPByIndex(IntPtr system, int index, ref IntPtr dsp);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_CreateChannelGroup(IntPtr system, string name, ref IntPtr channelgroup);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_PlaySound(IntPtr system, CHANNELINDEX channelid, IntPtr sound, bool paused, ref IntPtr channel);
        [DllImport(VERSION.dll)]
        public static extern RESULT FMOD_System_PlayDSP(IntPtr system, CHANNELINDEX channelid, IntPtr dsp, bool paused, ref IntPtr channel);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetChannel(IntPtr system, int channelid, ref IntPtr channel);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetMasterChannelGroup(IntPtr system, ref IntPtr channelgroup);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_SetReverbProperties(IntPtr system, ref REVERB_PROPERTIES prop);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetReverbProperties(IntPtr system, ref REVERB_PROPERTIES prop);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetDSPHead(IntPtr system, ref IntPtr dsp);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_AddDSP(IntPtr system, IntPtr dsp);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_LockDSP(IntPtr system);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_UnlockDSP(IntPtr system);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_SetRecordDriver(IntPtr system, int driver);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetRecordDriver(IntPtr system, ref int driver);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetRecordNumDrivers(IntPtr system, ref int numdrivers);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetRecordDriverName(IntPtr system, int id, StringBuilder name, int namelen);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetRecordPosition(IntPtr system, ref uint position);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_RecordStart(IntPtr system, IntPtr sound, bool loop);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_RecordStop(IntPtr system);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_IsRecording(IntPtr system, ref bool recording);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_CreateGeometry(IntPtr system, int maxPolygons, int maxVertices, ref IntPtr geometryf);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_SetGeometrySettings(IntPtr system, float maxWorldSize);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetGeometrySettings(IntPtr system, ref float maxWorldSize);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_LoadGeometry(IntPtr system, IntPtr data, int dataSize, ref IntPtr geometry);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_SetNetworkProxy(IntPtr system, string proxy);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetNetworkProxy(IntPtr system, StringBuilder proxy, int proxylen);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_SetNetworkTimeout(IntPtr system, int timeout);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetNetworkTimeout(IntPtr system, ref int timeout);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_SetUserData(IntPtr system, IntPtr userdata);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_System_GetUserData(IntPtr system, ref IntPtr userdata);

        #endregion

        #region wrapperinternal

        private IntPtr systemraw;

        public void setRaw(IntPtr system)
        {
            systemraw = new IntPtr();

            systemraw = system;
        }

        public IntPtr getRaw()
        {
            return systemraw;
        }

        #endregion
    }


    /*
        'Sound' API
    */
    public class Sound
    {
        public RESULT release()
        {
            try
            {
                GameLog.Client.Audio.DebugFormat("Trying FMOD_Sound_Release(soundraw)");
                return FMOD_Sound_Release(soundraw);
            }
            catch (Exception e)
            {
                GameLog.Client.Audio.Error(e);
                return RESULT.ERR_INVALID_PARAM;
            }
        }
        public RESULT getSystemObject(ref System system)
        {
            RESULT result = RESULT.OK;
            IntPtr systemraw = new IntPtr();

            try
            {
                result = FMOD_Sound_GetSystemObject(soundraw, ref systemraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (system == null)
            {
                System systemnew = new System();
                systemnew.setRaw(systemraw);
                system = systemnew;
            }
            else
            {
                system.setRaw(systemraw);
            }
            return result;
        }


        public RESULT @lock(uint offset, uint length, ref IntPtr ptr1, ref IntPtr ptr2, ref uint len1, ref uint len2)
        {
            return FMOD_Sound_Lock(soundraw, offset, length, ref ptr1, ref ptr2, ref len1, ref len2);
        }
        public RESULT unlock(IntPtr ptr1, IntPtr ptr2, uint len1, uint len2)
        {
            return FMOD_Sound_Unlock(soundraw, ptr1, ptr2, len1, len2);
        }
        public RESULT setDefaults(float frequency, float volume, float pan, int priority)
        {
            return FMOD_Sound_SetDefaults(soundraw, frequency, volume, pan, priority);
        }
        public RESULT getDefaults(ref float frequency, ref float volume, ref float pan, ref int priority)
        {
            return FMOD_Sound_GetDefaults(soundraw, ref frequency, ref volume, ref pan, ref priority);
        }
        public RESULT setVariations(float frequencyvar, float volumevar, float panvar)
        {
            return FMOD_Sound_SetVariations(soundraw, frequencyvar, volumevar, panvar);
        }
        public RESULT getVariations(ref float frequencyvar, ref float volumevar, ref float panvar)
        {
            return FMOD_Sound_GetVariations(soundraw, ref frequencyvar, ref volumevar, ref panvar);
        }
        public RESULT set3DMinMaxDistance(float min, float max)
        {
            return FMOD_Sound_Set3DMinMaxDistance(soundraw, min, max);
        }
        public RESULT get3DMinMaxDistance(ref float min, ref float max)
        {
            return FMOD_Sound_Get3DMinMaxDistance(soundraw, ref min, ref max);
        }
        public RESULT set3DConeSettings(float insideconeangle, float outsideconeangle, float outsidevolume)
        {
            return FMOD_Sound_Set3DConeSettings(soundraw, insideconeangle, outsideconeangle, outsidevolume);
        }
        public RESULT get3DConeSettings(ref float insideconeangle, ref float outsideconeangle, ref float outsidevolume)
        {
            return FMOD_Sound_Get3DConeSettings(soundraw, ref insideconeangle, ref outsideconeangle, ref outsidevolume);
        }
        public RESULT set3DCustomRolloff(ref VECTOR points, int numpoints)
        {
            return FMOD_Sound_Set3DCustomRolloff(soundraw, ref points, numpoints);
        }
        public RESULT get3DCustomRolloff(ref IntPtr points, ref int numpoints)
        {
            return FMOD_Sound_Get3DCustomRolloff(soundraw, ref points, ref numpoints);
        }
        public RESULT setSubSound(int index, Sound subsound)
        {
            IntPtr subsoundraw = subsound.getRaw();

            return FMOD_Sound_SetSubSound(soundraw, index, subsoundraw);
        }
        public RESULT getSubSound(int index, ref Sound subsound)
        {
            RESULT result = RESULT.OK;
            IntPtr subsoundraw = new IntPtr();

            try
            {
                result = FMOD_Sound_GetSubSound(soundraw, index, ref subsoundraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (subsound == null)
            {
                Sound subsoundnew = new Sound();
                subsoundnew.setRaw(subsoundraw);
                subsound = subsoundnew;
            }
            else
            {
                subsound.setRaw(subsoundraw);
            }

            return result;
        }
        public RESULT setSubSoundSentence(int[] subsoundlist, int numsubsounds)
        {
            return FMOD_Sound_SetSubSoundSentence(soundraw, subsoundlist, numsubsounds);
        }
        public RESULT getName(StringBuilder name, int namelen)
        {
            return FMOD_Sound_GetName(soundraw, name, namelen);
        }
        public RESULT getLength(ref uint length, TIMEUNIT lengthtype)
        {
            return FMOD_Sound_GetLength(soundraw, ref length, lengthtype);
        }
        public RESULT getFormat(ref SOUND_TYPE type, ref SOUND_FORMAT format, ref int channels, ref int bits)
        {
            return FMOD_Sound_GetFormat(soundraw, ref type, ref format, ref channels, ref bits);
        }
        public RESULT getNumSubSounds(ref int numsubsounds)
        {
            return FMOD_Sound_GetNumSubSounds(soundraw, ref numsubsounds);
        }
        public RESULT getNumTags(ref int numtags, ref int numtagsupdated)
        {
            return FMOD_Sound_GetNumTags(soundraw, ref numtags, ref numtagsupdated);
        }
        public RESULT getTag(string name, int index, ref TAG tag)
        {
            return FMOD_Sound_GetTag(soundraw, name, index, ref tag);
        }
        public RESULT getOpenState(ref OPENSTATE openstate, ref uint percentbuffered, ref bool starving)
        {
            return FMOD_Sound_GetOpenState(soundraw, ref openstate, ref percentbuffered, ref starving);
        }
        public RESULT readData(IntPtr buffer, uint lenbytes, ref uint read)
        {
            return FMOD_Sound_ReadData(soundraw, buffer, lenbytes, ref read);
        }
        public RESULT seekData(uint pcm)
        {
            return FMOD_Sound_SeekData(soundraw, pcm);
        }


        public RESULT setMode(MODE mode)
        {
            return FMOD_Sound_SetMode(soundraw, mode);
        }
        public RESULT getMode(ref MODE mode)
        {
            return FMOD_Sound_GetMode(soundraw, ref mode);
        }
        public RESULT setLoopCount(int loopcount)
        {
            return FMOD_Sound_SetLoopCount(soundraw, loopcount);
        }
        public RESULT getLoopCount(ref int loopcount)
        {
            return FMOD_Sound_GetLoopCount(soundraw, ref loopcount);
        }
        public RESULT setLoopPoints(uint loopstart, TIMEUNIT loopstarttype, uint loopend, TIMEUNIT loopendtype)
        {
            return FMOD_Sound_SetLoopPoints(soundraw, loopstart, loopstarttype, loopend, loopendtype);
        }
        public RESULT getLoopPoints(ref uint loopstart, TIMEUNIT loopstarttype, ref uint loopend, TIMEUNIT loopendtype)
        {
            return FMOD_Sound_GetLoopPoints(soundraw, ref loopstart, loopstarttype, ref loopend, loopendtype);
        }


        public RESULT setUserData(IntPtr userdata)
        {
            return FMOD_Sound_SetUserData(soundraw, userdata);
        }
        public RESULT getUserData(ref IntPtr userdata)
        {
            return FMOD_Sound_GetUserData(soundraw, ref userdata);
        }



        #region importfunctions

        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_Release(IntPtr sound);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_GetSystemObject(IntPtr sound, ref IntPtr system);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_Lock(IntPtr sound, uint offset, uint length, ref IntPtr ptr1, ref IntPtr ptr2, ref uint len1, ref uint len2);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_Unlock(IntPtr sound, IntPtr ptr1, IntPtr ptr2, uint len1, uint len2);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_SetDefaults(IntPtr sound, float frequency, float volume, float pan, int priority);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_GetDefaults(IntPtr sound, ref float frequency, ref float volume, ref float pan, ref int priority);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_SetVariations(IntPtr sound, float frequencyvar, float volumevar, float panvar);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_GetVariations(IntPtr sound, ref float frequencyvar, ref float volumevar, ref float panvar);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_Set3DMinMaxDistance(IntPtr sound, float min, float max);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_Get3DMinMaxDistance(IntPtr sound, ref float min, ref float max);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_Set3DConeSettings(IntPtr sound, float insideconeangle, float outsideconeangle, float outsidevolume);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_Get3DConeSettings(IntPtr sound, ref float insideconeangle, ref float outsideconeangle, ref float outsidevolume);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_Set3DCustomRolloff(IntPtr sound, ref VECTOR points, int numpoints);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_Get3DCustomRolloff(IntPtr sound, ref IntPtr points, ref int numpoints);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_SetSubSound(IntPtr sound, int index, IntPtr subsound);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_GetSubSound(IntPtr sound, int index, ref IntPtr subsound);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_SetSubSoundSentence(IntPtr sound, int[] subsoundlist, int numsubsounds);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_GetName(IntPtr sound, StringBuilder name, int namelen);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_GetLength(IntPtr sound, ref uint length, TIMEUNIT lengthtype);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_GetFormat(IntPtr sound, ref SOUND_TYPE type, ref SOUND_FORMAT format, ref int channels, ref int bits);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_GetNumSubSounds(IntPtr sound, ref int numsubsounds);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_GetNumTags(IntPtr sound, ref int numtags, ref int numtagsupdated);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_GetTag(IntPtr sound, string name, int index, ref TAG tag);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_GetOpenState(IntPtr sound, ref OPENSTATE openstate, ref uint percentbuffered, ref bool starving);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_ReadData(IntPtr sound, IntPtr buffer, uint lenbytes, ref uint read);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_SeekData(IntPtr sound, uint pcm);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_SetMode(IntPtr sound, MODE mode);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_GetMode(IntPtr sound, ref MODE mode);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_SetLoopCount(IntPtr sound, int loopcount);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_GetLoopCount(IntPtr sound, ref int loopcount);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_SetLoopPoints(IntPtr sound, uint loopstart, TIMEUNIT loopstarttype, uint loopend, TIMEUNIT loopendtype);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_GetLoopPoints(IntPtr sound, ref uint loopstart, TIMEUNIT loopstarttype, ref uint loopend, TIMEUNIT loopendtype);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_SetUserData(IntPtr sound, IntPtr userdata);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Sound_GetUserData(IntPtr sound, ref IntPtr userdata);

        #endregion

        #region wrapperinternal

        private IntPtr soundraw;

        public void setRaw(IntPtr sound)
        {
            soundraw = new IntPtr();
            soundraw = sound;
        }

        public IntPtr getRaw()
        {
            return soundraw;
        }

        #endregion
    }


    /*
        'Channel' API
    */
    public class Channel
    {
        public RESULT getSystemObject(ref System system)
        {
            RESULT result = RESULT.OK;
            IntPtr systemraw = new IntPtr();

            try
            {
                result = FMOD_Channel_GetSystemObject(channelraw, ref systemraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (system == null)
            {
                System systemnew = new System();
                systemnew.setRaw(systemraw);
                system = systemnew;
            }
            else
            {
                system.setRaw(systemraw);
            }

            return result;
        }


        public RESULT stop()
        {
            return FMOD_Channel_Stop(channelraw);
        }
        public RESULT setPaused(bool paused)
        {
            return FMOD_Channel_SetPaused(channelraw, paused);
        }
        public RESULT getPaused(ref bool paused)
        {
            return FMOD_Channel_GetPaused(channelraw, ref paused);
        }
        public RESULT setVolume(float volume)
        {
            return FMOD_Channel_SetVolume(channelraw, volume);
        }
        public RESULT getVolume(ref float volume)
        {
            return FMOD_Channel_GetVolume(channelraw, ref volume);
        }
        public RESULT setFrequency(float frequency)
        {
            return FMOD_Channel_SetFrequency(channelraw, frequency);
        }
        public RESULT getFrequency(ref float frequency)
        {
            return FMOD_Channel_GetFrequency(channelraw, ref frequency);
        }
        public RESULT setPan(float pan)
        {
            return FMOD_Channel_SetPan(channelraw, pan);
        }
        public RESULT getPan(ref float pan)
        {
            return FMOD_Channel_GetPan(channelraw, ref pan);
        }
        public RESULT setDelay(uint startdelay, uint enddelay)
        {
            return FMOD_Channel_SetDelay(channelraw, startdelay, enddelay);
        }
        public RESULT getDelay(ref uint startdelay, ref uint enddelay)
        {
            return FMOD_Channel_GetDelay(channelraw, ref startdelay, ref enddelay);
        }
        public RESULT setSpeakerMix(float frontleft, float frontright, float center, float lfe, float backleft, float backright, float sideleft, float sideright)
        {
            return FMOD_Channel_SetSpeakerMix(channelraw, frontleft, frontright, center, lfe, backleft, backright, sideleft, sideright);
        }
        public RESULT getSpeakerMix(ref float frontleft, ref float frontright, ref float center, ref float lfe, ref float backleft, ref float backright, ref float sideleft, ref float sideright)
        {
            return FMOD_Channel_GetSpeakerMix(channelraw, ref frontleft, ref frontright, ref center, ref lfe, ref backleft, ref backright, ref sideleft, ref sideright);
        }
        public RESULT setSpeakerLevels(SPEAKER speaker, float[] levels, int numlevels)
        {
            return FMOD_Channel_SetSpeakerLevels(channelraw, speaker, levels, numlevels);
        }
        public RESULT getSpeakerLevels(SPEAKER speaker, float[] levels, int numlevels)
        {
            return FMOD_Channel_GetSpeakerLevels(channelraw, speaker, levels, numlevels);
        }
        public RESULT setMute(bool mute)
        {
            return FMOD_Channel_SetMute(channelraw, mute);
        }
        public RESULT getMute(ref bool mute)
        {
            return FMOD_Channel_GetMute(channelraw, ref mute);
        }
        public RESULT setPriority(int priority)
        {
            return FMOD_Channel_SetPriority(channelraw, priority);
        }
        public RESULT getPriority(ref int priority)
        {
            return FMOD_Channel_GetPriority(channelraw, ref priority);
        }
        public RESULT setPosition(uint position, TIMEUNIT postype)
        {
            return FMOD_Channel_SetPosition(channelraw, position, postype);
        }
        public RESULT getPosition(ref uint position, TIMEUNIT postype)
        {
            return FMOD_Channel_GetPosition(channelraw, ref position, postype);
        }

        public RESULT setReverbProperties(ref REVERB_CHANNELPROPERTIES prop)
        {
            return FMOD_Channel_SetReverbProperties(channelraw, ref prop);
        }
        public RESULT getReverbProperties(ref REVERB_CHANNELPROPERTIES prop)
        {
            return FMOD_Channel_GetReverbProperties(channelraw, ref prop);
        }
        public RESULT setChannelGroup(ChannelGroup channelgroup)
        {
            return FMOD_Channel_SetChannelGroup(channelraw, channelgroup.getRaw());
        }
        public RESULT getChannelGroup(ref ChannelGroup channelgroup)
        {
            RESULT result = RESULT.OK;
            IntPtr channelgroupraw = new IntPtr();

            try
            {
                result = FMOD_Channel_GetChannelGroup(channelraw, ref channelgroupraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (channelgroup == null)
            {
                ChannelGroup channelgroupnew = new ChannelGroup();
                channelgroupnew.setRaw(channelgroupraw);
                channelgroup = channelgroupnew;
            }
            else
            {
                channelgroup.setRaw(channelgroupraw);
            }

            return result;
        }

        public RESULT setCallback(CHANNEL_CALLBACKTYPE type, CHANNEL_CALLBACK callback, int command)
        {
            return FMOD_Channel_SetCallback(channelraw, type, callback, command);
        }


        public RESULT set3DAttributes(ref VECTOR pos, ref VECTOR vel)
        {
            return FMOD_Channel_Set3DAttributes(channelraw, ref pos, ref vel);
        }
        public RESULT get3DAttributes(ref VECTOR pos, ref VECTOR vel)
        {
            return FMOD_Channel_Get3DAttributes(channelraw, ref pos, ref vel);
        }
        public RESULT set3DMinMaxDistance(float mindistance, float maxdistance)
        {
            return FMOD_Channel_Set3DMinMaxDistance(channelraw, mindistance, maxdistance);
        }
        public RESULT get3DMinMaxDistance(ref float mindistance, ref float maxdistance)
        {
            return FMOD_Channel_Get3DMinMaxDistance(channelraw, ref mindistance, ref maxdistance);
        }
        public RESULT set3DConeSettings(float insideconeangle, float outsideconeangle, float outsidevolume)
        {
            return FMOD_Channel_Set3DConeSettings(channelraw, insideconeangle, outsideconeangle, outsidevolume);
        }
        public RESULT get3DConeSettings(ref float insideconeangle, ref float outsideconeangle, ref float outsidevolume)
        {
            return FMOD_Channel_Get3DConeSettings(channelraw, ref insideconeangle, ref outsideconeangle, ref outsidevolume);
        }
        public RESULT set3DConeOrientation(ref VECTOR orientation)
        {
            return FMOD_Channel_Set3DConeOrientation(channelraw, ref orientation);
        }
        public RESULT get3DConeOrientation(ref VECTOR orientation)
        {
            return FMOD_Channel_Get3DConeOrientation(channelraw, ref orientation);
        }
        public RESULT set3DCustomRolloff(ref VECTOR points, int numpoints)
        {
            return FMOD_Channel_Set3DCustomRolloff(channelraw, ref points, numpoints);
        }
        public RESULT get3DCustomRolloff(ref IntPtr points, ref int numpoints)
        {
            return FMOD_Channel_Get3DCustomRolloff(channelraw, ref points, ref numpoints);
        }
        public RESULT set3DOcclusion(float directOcclusion, float reverbOcclusion)
        {
            return FMOD_Channel_Set3DOcclusion(channelraw, directOcclusion, reverbOcclusion);
        }
        public RESULT get3DOcclusion(ref float directOcclusion, ref float reverbOcclusion)
        {
            return FMOD_Channel_Get3DOcclusion(channelraw, ref directOcclusion, ref reverbOcclusion);
        }


        public RESULT isPlaying(ref bool isplaying)
        {
            return FMOD_Channel_IsPlaying(channelraw, ref isplaying);
        }
        public RESULT isVirtual(ref bool isvirtual)
        {
            return FMOD_Channel_IsVirtual(channelraw, ref isvirtual);
        }
        public RESULT getAudibility(ref float audibility)
        {
            return FMOD_Channel_GetAudibility(channelraw, ref audibility);
        }
        public RESULT getCurrentSound(ref Sound sound)
        {
            RESULT result = RESULT.OK;
            IntPtr soundraw = new IntPtr();

            try
            {
                result = FMOD_Channel_GetCurrentSound(channelraw, ref soundraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (sound == null)
            {
                Sound soundnew = new Sound();
                soundnew.setRaw(soundraw);
                sound = soundnew;
            }
            else
            {
                sound.setRaw(soundraw);
            }

            return result;
        }
        public RESULT getSpectrum(float[] spectrumarray, int numvalues, int channeloffset, DSP_FFT_WINDOW windowtype)
        {
            return FMOD_Channel_GetSpectrum(channelraw, spectrumarray, numvalues, channeloffset, windowtype);
        }
        public RESULT getWaveData(float[] wavearray, int numvalues, int channeloffset)
        {
            return FMOD_Channel_GetWaveData(channelraw, wavearray, numvalues, channeloffset);
        }
        public RESULT getIndex(ref int index)
        {
            return FMOD_Channel_GetIndex(channelraw, ref index);
        }

        public RESULT getDSPHead(ref DSP dsp)
        {
            RESULT result = RESULT.OK;
            IntPtr dspraw = new IntPtr();
            DSP dspnew = null;

            try
            {
                result = FMOD_Channel_GetDSPHead(channelraw, ref dspraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            dspnew = new DSP();
            dspnew.setRaw(dspraw);
            dsp = dspnew;

            return result;
        }
        public RESULT addDSP(DSP dsp)
        {
            return FMOD_Channel_AddDSP(channelraw, dsp.getRaw());
        }


        public RESULT setMode(MODE mode)
        {
            return FMOD_Channel_SetMode(channelraw, mode);
        }
        public RESULT getMode(ref MODE mode)
        {
            return FMOD_Channel_GetMode(channelraw, ref mode);
        }
        public RESULT setLoopCount(int loopcount)
        {
            return FMOD_Channel_SetLoopCount(channelraw, loopcount);
        }
        public RESULT getLoopCount(ref int loopcount)
        {
            return FMOD_Channel_GetLoopCount(channelraw, ref loopcount);
        }
        public RESULT setLoopPoints(uint loopstart, TIMEUNIT loopstarttype, uint loopend, TIMEUNIT loopendtype)
        {
            return FMOD_Channel_SetLoopPoints(channelraw, loopstart, loopstarttype, loopend, loopendtype);
        }
        public RESULT getLoopPoints(ref uint loopstart, TIMEUNIT loopstarttype, ref uint loopend, TIMEUNIT loopendtype)
        {
            return FMOD_Channel_GetLoopPoints(channelraw, ref loopstart, loopstarttype, ref loopend, loopendtype);
        }


        public RESULT setUserData(IntPtr userdata)
        {
            return FMOD_Channel_SetUserData(channelraw, userdata);
        }
        public RESULT getUserData(ref IntPtr userdata)
        {
            return FMOD_Channel_GetUserData(channelraw, ref userdata);
        }


        #region importfunctions

        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_GetSystemObject(IntPtr channel, ref IntPtr system);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_Stop(IntPtr channel);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_SetPaused(IntPtr channel, bool paused);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_GetPaused(IntPtr channel, ref bool paused);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_SetVolume(IntPtr channel, float volume);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_GetVolume(IntPtr channel, ref float volume);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_SetFrequency(IntPtr channel, float frequency);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_GetFrequency(IntPtr channel, ref float frequency);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_SetPan(IntPtr channel, float pan);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_GetPan(IntPtr channel, ref float pan);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_SetDelay(IntPtr channel, uint startdelay, uint enddelay);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_GetDelay(IntPtr channel, ref uint startdelay, ref uint enddelay);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_SetSpeakerMix(IntPtr channel, float frontleft, float frontright, float center, float lfe, float backleft, float backright, float sideleft, float sideright);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_GetSpeakerMix(IntPtr channel, ref float frontleft, ref float frontright, ref float center, ref float lfe, ref float backleft, ref float backright, ref float sideleft, ref float sideright);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_SetSpeakerLevels(IntPtr channel, SPEAKER speaker, float[] levels, int numlevels);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_GetSpeakerLevels(IntPtr channel, SPEAKER speaker, [MarshalAs(UnmanagedType.LPArray)] float[] levels, int numlevels);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_SetMute(IntPtr channel, bool mute);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_GetMute(IntPtr channel, ref bool mute);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_SetPriority(IntPtr channel, int priority);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_GetPriority(IntPtr channel, ref int priority);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_Set3DAttributes(IntPtr channel, ref VECTOR pos, ref VECTOR vel);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_Get3DAttributes(IntPtr channel, ref VECTOR pos, ref VECTOR vel);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_Set3DMinMaxDistance(IntPtr channel, float mindistance, float maxdistance);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_Get3DMinMaxDistance(IntPtr channel, ref float mindistance, ref float maxdistance);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_Set3DConeSettings(IntPtr channel, float insideconeangle, float outsideconeangle, float outsidevolume);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_Get3DConeSettings(IntPtr channel, ref float insideconeangle, ref float outsideconeangle, ref float outsidevolume);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_Set3DConeOrientation(IntPtr channel, ref VECTOR orientation);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_Get3DConeOrientation(IntPtr channel, ref VECTOR orientation);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_Set3DCustomRolloff(IntPtr channel, ref VECTOR points, int numpoints);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_Get3DCustomRolloff(IntPtr channel, ref IntPtr points, ref int numpoints);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_Set3DOcclusion(IntPtr channel, float directOcclusion, float reverbOcclusion);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_Get3DOcclusion(IntPtr channel, ref float directOcclusion, ref float reverbOcclusion);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_SetReverbProperties(IntPtr channel, ref REVERB_CHANNELPROPERTIES prop);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_GetReverbProperties(IntPtr channel, ref REVERB_CHANNELPROPERTIES prop);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_SetChannelGroup(IntPtr channel, IntPtr channelgroup);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_GetChannelGroup(IntPtr channel, ref IntPtr channelgroup);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_IsPlaying(IntPtr channel, ref bool isplaying);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_IsVirtual(IntPtr channel, ref bool isvirtual);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_GetAudibility(IntPtr channel, ref float audibility);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_GetCurrentSound(IntPtr channel, ref IntPtr sound);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_GetSpectrum(IntPtr channel, [MarshalAs(UnmanagedType.LPArray)] float[] spectrumarray, int numvalues, int channeloffset, DSP_FFT_WINDOW windowtype);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_GetWaveData(IntPtr channel, [MarshalAs(UnmanagedType.LPArray)] float[] wavearray, int numvalues, int channeloffset);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_GetIndex(IntPtr channel, ref int index);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_SetCallback(IntPtr channel, CHANNEL_CALLBACKTYPE type, CHANNEL_CALLBACK callback, int command);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_SetPosition(IntPtr channel, uint position, TIMEUNIT postype);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_GetPosition(IntPtr channel, ref uint position, TIMEUNIT postype);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_GetDSPHead(IntPtr channel, ref IntPtr dsp);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_AddDSP(IntPtr channel, IntPtr dsp);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_SetMode(IntPtr channel, MODE mode);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_GetMode(IntPtr channel, ref MODE mode);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_SetLoopCount(IntPtr channel, int loopcount);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_GetLoopCount(IntPtr channel, ref int loopcount);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_SetLoopPoints(IntPtr channel, uint loopstart, TIMEUNIT loopstarttype, uint loopend, TIMEUNIT loopendtype);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_GetLoopPoints(IntPtr channel, ref uint loopstart, TIMEUNIT loopstarttype, ref uint loopend, TIMEUNIT loopendtype);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_SetUserData(IntPtr channel, IntPtr userdata);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Channel_GetUserData(IntPtr channel, ref IntPtr userdata);
        #endregion

        #region wrapperinternal

        private IntPtr channelraw;

        public void setRaw(IntPtr channel)
        {
            channelraw = new IntPtr();

            channelraw = channel;
        }

        public IntPtr getRaw()
        {
            return channelraw;
        }

        #endregion
    }


    /*
        'ChannelGroup' API
    */
    public class ChannelGroup
    {
        public RESULT release()
        {
            return FMOD_ChannelGroup_Release(channelgroupraw);
        }
        public RESULT getSystemObject(ref System system)
        {
            RESULT result = RESULT.OK;
            IntPtr systemraw = new IntPtr();

            try
            {
                result = FMOD_ChannelGroup_GetSystemObject(channelgroupraw, ref systemraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (system == null)
            {
                System systemnew = new System();
                systemnew.setRaw(systemraw);
                system = systemnew;
            }
            else
            {
                system.setRaw(systemraw);
            }

            return result;
        }


        // Channelgroup scale values.  (scales the current volume or pitch of all channels and channel groups, DOESN'T overwrite)
        public RESULT setVolume(float volume)
        {
            return FMOD_ChannelGroup_SetVolume(channelgroupraw, volume);
        }
        public RESULT getVolume(ref float volume)
        {
            return FMOD_ChannelGroup_GetVolume(channelgroupraw, ref volume);
        }
        public RESULT setPitch(float pitch)
        {
            return FMOD_ChannelGroup_SetPitch(channelgroupraw, pitch);
        }
        public RESULT getPitch(ref float pitch)
        {
            return FMOD_ChannelGroup_GetPitch(channelgroupraw, ref pitch);
        }


        // Channelgroup override values.  (recursively overwrites whatever settings the channels had)
        public RESULT stop()
        {
            return FMOD_ChannelGroup_Stop(channelgroupraw);
        }
        public RESULT overridePaused(bool paused)
        {
            return FMOD_ChannelGroup_OverridePaused(channelgroupraw, paused);
        }
        public RESULT overrideVolume(float volume)
        {
            return FMOD_ChannelGroup_OverrideVolume(channelgroupraw, volume);
        }
        public RESULT overrideFrequency(float frequency)
        {
            return FMOD_ChannelGroup_OverrideFrequency(channelgroupraw, frequency);
        }
        public RESULT overridePan(float pan)
        {
            return FMOD_ChannelGroup_OverridePan(channelgroupraw, pan);
        }
        public RESULT overrideMute(bool mute)
        {
            return FMOD_ChannelGroup_OverrideMute(channelgroupraw, mute);
        }
        public RESULT overrideReverbProperties(ref REVERB_CHANNELPROPERTIES prop)
        {
            return FMOD_ChannelGroup_OverrideReverbProperties(channelgroupraw, ref prop);
        }
        public RESULT override3DAttributes(ref VECTOR pos, ref VECTOR vel)
        {
            return FMOD_ChannelGroup_Override3DAttributes(channelgroupraw, ref pos, ref vel);
        }
        public RESULT overrideSpeakerMix(float frontleft, float frontright, float center, float lfe, float backleft, float backright, float sideleft, float sideright)
        {
            return FMOD_ChannelGroup_OverrideSpeakerMix(channelgroupraw, frontleft, frontright, center, lfe, backleft, backright, sideleft, sideright);
        }


        // Nested channel groups.
        public RESULT addGroup(ChannelGroup group)
        {
            return FMOD_ChannelGroup_AddGroup(channelgroupraw, group.getRaw());
        }
        public RESULT getNumGroups(ref int numgroups)
        {
            return FMOD_ChannelGroup_GetNumGroups(channelgroupraw, ref numgroups);
        }
        public RESULT getGroup(int index, ref ChannelGroup group)
        {
            RESULT result = RESULT.OK;
            IntPtr channelraw = new IntPtr();

            try
            {
                result = FMOD_ChannelGroup_GetGroup(channelgroupraw, index, ref channelraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (group == null)
            {
                ChannelGroup channelnew = new ChannelGroup();
                channelnew.setRaw(channelraw);
                group = channelnew;
            }
            else
            {
                group.setRaw(channelraw);
            }

            return result;
        }

        // DSP functionality only for channel groups playing sounds created with FMOD_SOFTWARE.
        public RESULT getDSPHead(ref DSP dsp)
        {
            RESULT result = RESULT.OK;
            IntPtr dspraw = new IntPtr();

            try
            {
                result = FMOD_ChannelGroup_GetDSPHead(channelgroupraw, ref dspraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (dsp == null)
            {
                DSP dspnew = new DSP();
                dspnew.setRaw(dspraw);
                dsp = dspnew;
            }
            else
            {
                dsp.setRaw(dspraw);
            }

            return result;
        }

        public RESULT addDSP(DSP dsp)
        {
            return FMOD_ChannelGroup_AddDSP(channelgroupraw, dsp.getRaw());
        }


        // Information only functions.
        public RESULT getName(StringBuilder name, int namelen)
        {
            return FMOD_ChannelGroup_GetName(channelgroupraw, name, namelen);
        }
        public RESULT getNumChannels(ref int numchannels)
        {
            return FMOD_ChannelGroup_GetNumChannels(channelgroupraw, ref numchannels);
        }
        public RESULT getChannel(int index, ref Channel channel)
        {
            RESULT result = RESULT.OK;
            IntPtr channelraw = new IntPtr();

            try
            {
                result = FMOD_ChannelGroup_GetChannel(channelgroupraw, index, ref channelraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (channel == null)
            {
                Channel channelnew = new Channel();
                channelnew.setRaw(channelraw);
                channel = channelnew;
            }
            else
            {
                channel.setRaw(channelraw);
            }

            return result;
        }
        public RESULT getSpectrum(float[] spectrumarray, int numvalues, int channeloffset, DSP_FFT_WINDOW windowtype)
        {
            return FMOD_ChannelGroup_GetSpectrum(channelgroupraw, spectrumarray, numvalues, channeloffset, windowtype);
        }
        public RESULT getWaveData(float[] wavearray, int numvalues, int channeloffset)
        {
            return FMOD_ChannelGroup_GetWaveData(channelgroupraw, wavearray, numvalues, channeloffset);
        }


        // Userdata set/get.
        public RESULT setUserData(IntPtr userdata)
        {
            return FMOD_ChannelGroup_SetUserData(channelgroupraw, userdata);
        }
        public RESULT getUserData(ref IntPtr userdata)
        {
            return FMOD_ChannelGroup_GetUserData(channelgroupraw, ref userdata);
        }

        #region importfunctions


        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_Release(IntPtr channelgroup);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_GetSystemObject(IntPtr channelgroup, ref IntPtr system);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_SetVolume(IntPtr channelgroup, float volume);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_GetVolume(IntPtr channelgroup, ref float volume);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_SetPitch(IntPtr channelgroup, float pitch);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_GetPitch(IntPtr channelgroup, ref float pitch);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_Stop(IntPtr channelgroup);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_OverridePaused(IntPtr channelgroup, bool paused);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_OverrideVolume(IntPtr channelgroup, float volume);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_OverrideFrequency(IntPtr channelgroup, float frequency);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_OverridePan(IntPtr channelgroup, float pan);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_OverrideMute(IntPtr channelgroup, bool mute);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_OverrideReverbProperties(IntPtr channelgroup, ref REVERB_CHANNELPROPERTIES prop);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_Override3DAttributes(IntPtr channelgroup, ref VECTOR pos, ref VECTOR vel);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_OverrideSpeakerMix(IntPtr channelgroup, float frontleft, float frontright, float center, float lfe, float backleft, float backright, float sideleft, float sideright);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_AddGroup(IntPtr channelgroup, IntPtr group);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_GetNumGroups(IntPtr channelgroup, ref int numgroups);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_GetGroup(IntPtr channelgroup, int index, ref IntPtr group);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_GetDSPHead(IntPtr channelgroup, ref IntPtr dsp);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_AddDSP(IntPtr channelgroup, IntPtr dsp);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_GetName(IntPtr channelgroup, StringBuilder name, int namelen);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_GetNumChannels(IntPtr channelgroup, ref int numchannels);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_GetChannel(IntPtr channelgroup, int index, ref IntPtr channel);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_GetSpectrum(IntPtr channelgroup, [MarshalAs(UnmanagedType.LPArray)] float[] spectrumarray, int numvalues, int channeloffset, DSP_FFT_WINDOW windowtype);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_GetWaveData(IntPtr channelgroup, [MarshalAs(UnmanagedType.LPArray)] float[] wavearray, int numvalues, int channeloffset);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_SetUserData(IntPtr channelgroup, IntPtr userdata);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_ChannelGroup_GetUserData(IntPtr channelgroup, ref IntPtr userdata);

        #endregion

        #region wrapperinternal

        private IntPtr channelgroupraw;

        public void setRaw(IntPtr channelgroup)
        {
            channelgroupraw = new IntPtr();

            channelgroupraw = channelgroup;
        }

        public IntPtr getRaw()
        {
            return channelgroupraw;
        }

        #endregion
    }


    /*
        'DSP' API
    */
    public class DSP
    {
        public RESULT release()
        {
            return FMOD_DSP_Release(dspraw);
        }
        public RESULT getSystemObject(ref System system)
        {
            RESULT result = RESULT.OK;
            IntPtr systemraw = new IntPtr();

            try
            {
                result = FMOD_DSP_GetSystemObject(dspraw, ref systemraw);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (system == null)
            {
                System systemnew = new System();
                systemnew.setRaw(dspraw);
                system = systemnew;
            }
            else
            {
                system.setRaw(systemraw);
            }

            return result;
        }


        public RESULT addInput(DSP target)
        {
            return FMOD_DSP_AddInput(dspraw, target.getRaw());
        }
        public RESULT disconnectFrom(DSP target)
        {
            return FMOD_DSP_DisconnectFrom(dspraw, target.getRaw());
        }
        public RESULT remove()
        {
            return FMOD_DSP_Remove(dspraw);
        }
        public RESULT getNumInputs(ref int numinputs)
        {
            return FMOD_DSP_GetNumInputs(dspraw, ref numinputs);
        }
        public RESULT getNumOutputs(ref int numoutputs)
        {
            return FMOD_DSP_GetNumOutputs(dspraw, ref numoutputs);
        }
        public RESULT getInput(int index, ref DSP input)
        {
            RESULT result = RESULT.OK;
            IntPtr dsprawnew = new IntPtr();

            try
            {
                result = FMOD_DSP_GetInput(dspraw, index, ref dsprawnew);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (input == null)
            {
                DSP dspnew = new DSP();
                dspnew.setRaw(dsprawnew);
                input = dspnew;
            }
            else
            {
                input.setRaw(dsprawnew);
            }

            return result;
        }
        public RESULT getOutput(int index, ref DSP output)
        {
            RESULT result = RESULT.OK;
            IntPtr dsprawnew = new IntPtr();

            try
            {
                result = FMOD_DSP_GetOutput(dspraw, index, ref dsprawnew);
            }
            catch
            {
                result = RESULT.ERR_INVALID_PARAM;
            }
            if (result != RESULT.OK)
            {
                return result;
            }

            if (output == null)
            {
                DSP dspnew = new DSP();
                dspnew.setRaw(dsprawnew);
                output = dspnew;
            }
            else
            {
                output.setRaw(dsprawnew);
            }

            return result;
        }
        public RESULT setInputMix(int index, float volume)
        {
            return FMOD_DSP_SetInputMix(dspraw, index, volume);
        }
        public RESULT getInputMix(int index, ref float volume)
        {
            return FMOD_DSP_GetInputMix(dspraw, index, ref volume);
        }
        public RESULT setInputLevels(int index, SPEAKER speaker, float[] levels, int numlevels)
        {
            return FMOD_DSP_SetInputLevels(dspraw, index, speaker, levels, numlevels);
        }
        public RESULT getInputLevels(int index, SPEAKER speaker, float[] levels, int numlevels)
        {
            return FMOD_DSP_GetInputLevels(dspraw, index, speaker, levels, numlevels);
        }
        public RESULT setActive(bool active)
        {
            return FMOD_DSP_SetActive(dspraw, active);
        }
        public RESULT getActive(ref bool active)
        {
            return FMOD_DSP_GetActive(dspraw, ref active);
        }
        public RESULT setBypass(bool bypass)
        {
            return FMOD_DSP_SetBypass(dspraw, bypass);
        }
        public RESULT getBypass(ref bool bypass)
        {
            return FMOD_DSP_GetBypass(dspraw, ref bypass);
        }
        public RESULT reset()
        {
            return FMOD_DSP_Reset(dspraw);
        }


        public RESULT setParameter(int index, float val)
        {
            return FMOD_DSP_SetParameter(dspraw, index, val);
        }
        public RESULT getParameter(int index, ref float val, StringBuilder valuestr, int valuestrlen)
        {
            return FMOD_DSP_GetParameter(dspraw, index, ref val, valuestr, valuestrlen);
        }
        public RESULT getNumParameters(ref int numparams)
        {
            return FMOD_DSP_GetNumParameters(dspraw, ref numparams);
        }
        public RESULT getParameterInfo(int index, StringBuilder name, StringBuilder label, StringBuilder description, int descriptionlen, ref float min, ref float max)
        {
            return FMOD_DSP_GetParameterInfo(dspraw, index, name, label, description, descriptionlen, ref min, ref max);
        }
        public RESULT showConfigDialog(IntPtr hwnd, bool show)
        {
            return FMOD_DSP_ShowConfigDialog(dspraw, hwnd, show);
        }


        public RESULT getInfo(StringBuilder name, ref uint version, ref int channels, ref int configwidth, ref int configheight)
        {
            return FMOD_DSP_GetInfo(dspraw, name, ref version, ref channels, ref configwidth, ref configheight);
        }
        public RESULT setDefaults(float frequency, float volume, float pan, int priority)
        {
            return FMOD_DSP_SetDefaults(dspraw, frequency, volume, pan, priority);
        }
        public RESULT getDefaults(ref float frequency, ref float volume, ref float pan, ref int priority)
        {
            return FMOD_DSP_GetDefaults(dspraw, ref frequency, ref volume, ref pan, ref priority);
        }


        public RESULT setUserData(IntPtr userdata)
        {
            return FMOD_DSP_SetUserData(dspraw, userdata);
        }
        public RESULT getUserData(ref IntPtr userdata)
        {
            return FMOD_DSP_GetUserData(dspraw, ref userdata);
        }



        #region importfunctions

        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_Release(IntPtr dsp);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_GetSystemObject(IntPtr dsp, ref IntPtr system);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_AddInput(IntPtr dsp, IntPtr target);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_DisconnectFrom(IntPtr dsp, IntPtr target);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_Remove(IntPtr dsp);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_GetNumInputs(IntPtr dsp, ref int numinputs);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_GetNumOutputs(IntPtr dsp, ref int numoutputs);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_GetInput(IntPtr dsp, int index, ref IntPtr input);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_GetOutput(IntPtr dsp, int index, ref IntPtr output);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_SetInputMix(IntPtr dsp, int index, float volume);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_GetInputMix(IntPtr dsp, int index, ref float volume);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_SetActive(IntPtr dsp, bool active);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_GetActive(IntPtr dsp, ref bool active);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_SetInputLevels(IntPtr dsp, int index, SPEAKER speaker, float[] levels, int numlevels);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_GetInputLevels(IntPtr dsp, int index, SPEAKER speaker, float[] levels, int numlevels);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_SetBypass(IntPtr dsp, bool bypass);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_GetBypass(IntPtr dsp, ref bool bypass);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_Reset(IntPtr dsp);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_SetParameter(IntPtr dsp, int index, float val);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_GetParameter(IntPtr dsp, int index, ref float val, StringBuilder valuestr, int valuestrlen);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_GetNumParameters(IntPtr dsp, ref int numparams);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_GetParameterInfo(IntPtr dsp, int index, StringBuilder name, StringBuilder label, StringBuilder description, int descriptionlen, ref float min, ref float max);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_ShowConfigDialog(IntPtr dsp, IntPtr hwnd, bool show);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_GetInfo(IntPtr dsp, StringBuilder name, ref uint version, ref int channels, ref int configwidth, ref int configheight);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_SetDefaults(IntPtr dsp, float frequency, float volume, float pan, int priority);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_GetDefaults(IntPtr dsp, ref float frequency, ref float volume, ref float pan, ref int priority);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_SetUserData(IntPtr dsp, IntPtr userdata);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_DSP_GetUserData(IntPtr dsp, ref IntPtr userdata);

        #endregion

        #region wrapperinternal

        private IntPtr dspraw;

        public void setRaw(IntPtr dsp)
        {
            dspraw = new IntPtr();

            dspraw = dsp;
        }

        public IntPtr getRaw()
        {
            return dspraw;
        }

        #endregion
    }


    /*
        'Geometry' API
    */
    public class Geometry
    {
        public RESULT release()
        {
            return FMOD_Geometry_Release(geometryraw);
        }
        public RESULT addPolygon(float directOcclusion, float reverbOcclusion, bool doubleSided, int numVertices, ref VECTOR vertices, ref int polygonIndex)
        {
            return FMOD_Geometry_AddPolygon(geometryraw, directOcclusion, reverbOcclusion, doubleSided, numVertices, ref vertices, ref polygonIndex);
        }


        public RESULT getNumPolygons(ref int numPolygons)
        {
            return FMOD_Geometry_GetNumPolygons(geometryraw, ref numPolygons);
        }
        public RESULT getMaxPolygons(ref int maxPolygons, ref int maxVertices)
        {
            return FMOD_Geometry_GetMaxPolygons(geometryraw, ref maxPolygons, ref maxVertices);
        }
        public RESULT getPolygonNumVertices(int polygonIndex, ref int numVertices)
        {
            return FMOD_Geometry_GetPolygonNumVertices(geometryraw, polygonIndex, ref numVertices);
        }
        public RESULT setPolygonVertex(int polygonIndex, int vertexIndex, ref VECTOR vertex)
        {
            return FMOD_Geometry_SetPolygonVertex(geometryraw, polygonIndex, vertexIndex, ref vertex);
        }
        public RESULT getPolygonVertex(int polygonIndex, int vertexIndex, ref VECTOR vertex)
        {
            return FMOD_Geometry_GetPolygonVertex(geometryraw, polygonIndex, vertexIndex, ref vertex);
        }
        public RESULT setPolygonAttributes(int polygonIndex, float directOcclusion, float reverbOcclusion, bool doubleSided)
        {
            return FMOD_Geometry_SetPolygonAttributes(geometryraw, polygonIndex, directOcclusion, reverbOcclusion, doubleSided);
        }
        public RESULT getPolygonAttributes(int polygonIndex, ref float directOcclusion, ref float reverbOcclusion, ref bool doubleSided)
        {
            return FMOD_Geometry_GetPolygonAttributes(geometryraw, polygonIndex, ref directOcclusion, ref reverbOcclusion, ref doubleSided);
        }

        public RESULT setActive(bool active)
        {
            return FMOD_Geometry_SetActive(geometryraw, active);
        }
        public RESULT getActive(ref bool active)
        {
            return FMOD_Geometry_GetActive(geometryraw, ref active);
        }
        public RESULT setRotation(ref VECTOR forward, ref VECTOR up)
        {
            return FMOD_Geometry_SetRotation(geometryraw, ref forward, ref up);
        }
        public RESULT getRotation(ref VECTOR forward, ref VECTOR up)
        {
            return FMOD_Geometry_GetRotation(geometryraw, ref forward, ref up);
        }
        public RESULT setPosition(ref VECTOR position)
        {
            return FMOD_Geometry_SetPosition(geometryraw, ref position);
        }
        public RESULT getPosition(ref VECTOR position)
        {
            return FMOD_Geometry_GetPosition(geometryraw, ref position);
        }
        public RESULT setScale(ref VECTOR scale)
        {
            return FMOD_Geometry_SetScale(geometryraw, ref scale);
        }
        public RESULT getScale(ref VECTOR scale)
        {
            return FMOD_Geometry_GetScale(geometryraw, ref scale);
        }
        public RESULT save(IntPtr data, ref int datasize)
        {
            return FMOD_Geometry_Save(geometryraw, data, ref datasize);
        }


        public RESULT setUserData(IntPtr userdata)
        {
            return FMOD_Geometry_SetUserData(geometryraw, userdata);
        }
        public RESULT getUserData(ref IntPtr userdata)
        {
            return FMOD_Geometry_GetUserData(geometryraw, ref userdata);
        }



        #region importfunctions

        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_Release(IntPtr geometry);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_AddPolygon(IntPtr geometry, float directOcclusion, float reverbOcclusion, bool doubleSided, int numVertices, ref VECTOR vertices, ref int polygonIndex);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_GetNumPolygons(IntPtr geometry, ref int numPolygons);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_GetMaxPolygons(IntPtr geometry, ref int maxPolygons, ref int maxVertices);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_GetPolygonNumVertices(IntPtr geometry, int polygonIndex, ref int numVertices);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_SetPolygonVertex(IntPtr geometry, int polygonIndex, int vertexIndex, ref VECTOR vertex);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_GetPolygonVertex(IntPtr geometry, int polygonIndex, int vertexIndex, ref VECTOR vertex);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_SetPolygonAttributes(IntPtr geometry, int polygonIndex, float directOcclusion, float reverbOcclusion, bool doubleSided);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_GetPolygonAttributes(IntPtr geometry, int polygonIndex, ref float directOcclusion, ref float reverbOcclusion, ref bool doubleSided);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_Flush(IntPtr geometry);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_SetActive(IntPtr gemoetry, bool active);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_GetActive(IntPtr gemoetry, ref bool active);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_SetRotation(IntPtr geometry, ref VECTOR forward, ref VECTOR up);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_GetRotation(IntPtr geometry, ref VECTOR forward, ref VECTOR up);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_SetPosition(IntPtr geometry, ref VECTOR position);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_GetPosition(IntPtr geometry, ref VECTOR position);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_SetScale(IntPtr geometry, ref VECTOR scale);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_GetScale(IntPtr geometry, ref VECTOR scale);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_Save(IntPtr geometry, IntPtr data, ref int datasize);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_SetUserData(IntPtr geometry, IntPtr userdata);
        [DllImport(VERSION.dll)]
        private static extern RESULT FMOD_Geometry_GetUserData(IntPtr geometry, ref IntPtr userdata);

        #endregion

        #region wrapperinternal

        private IntPtr geometryraw;

        public void setRaw(IntPtr geometry)
        {
            geometryraw = new IntPtr();

            geometryraw = geometry;
        }

        public IntPtr getRaw()
        {
            return geometryraw;
        }

        #endregion
    }
}