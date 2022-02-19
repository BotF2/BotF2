
/* ============================================================================================= = */
/* FMOD Ex - Error string header file. Copyright (c), Firelight Technologies Pty, Ltd. 2004-2005.  */
/*                                                                                                 */
/* Use this header if you want to store or display a string version / english explanation of       */
/* the FMOD error codes.                                                                           */
/*                                                                                                 */
/* =============================================================================================== */

namespace FMOD
{
    class Error
    {
        public static string String(RESULT errcode)
        {
            switch (errcode)
            {
                case RESULT.ERR_ALREADYLOCKED: return "Tried to call lock a second time before unlock was called. ";
                case RESULT.ERR_BADCOMMAND: return "Tried to call a function on a data type that does not allow this type of functionality (ie calling Sound::lock on a streaming sound). ";
                case RESULT.ERR_CDDA_DRIVERS: return "Neither NTSCSI nor ASPI could be initialised. ";
                case RESULT.ERR_CDDA_INIT: return "An error occurred while initialising the CDDA subsystem. ";
                case RESULT.ERR_CDDA_INVALID_DEVICE: return "Couldn't find the specified device. ";
                case RESULT.ERR_CDDA_NOAUDIO: return "No audio tracks on the specified disc. ";
                case RESULT.ERR_CDDA_NODEVICES: return "No CD/DVD devices were found. ";
                case RESULT.ERR_CDDA_NODISC: return "No disc present in the specified drive. ";
                case RESULT.ERR_CDDA_READ: return "A CDDA read error occurred. ";
                case RESULT.ERR_CHANNEL_ALLOC: return "Error trying to allocate a channel. ";
                case RESULT.ERR_CHANNEL_STOLEN: return "The specified channel has been reused to play another sound. ";
                case RESULT.ERR_COM: return "A Win32 COM related error occured. COM failed to initialize or a QueryInterface failed meaning a Windows codec or driver was not installed properly. ";
                case RESULT.ERR_DMA: return "DMA Failure.  See debug output for more information. ";
                case RESULT.ERR_DSP_CONNECTION: return "DSP connection error.  Connection possibly caused a cyclic dependancy. ";
                case RESULT.ERR_DSP_FORMAT: return "DSP Format error.  A DSP unit may have attempted to connect to this network with the wrong format. ";
                case RESULT.ERR_DSP_NOTFOUND: return "DSP connection error.  Couldn't find the DSP unit specified. ";
                case RESULT.ERR_DSP_RUNNING: return "DSP error.  Cannot perform this operation while the network is in the middle of running.  This will most likely happen if a connection or disconnection is attempted in a DSP callback. ";
                case RESULT.ERR_DSP_TOOMANYCONNECTIONS: return "DSP connection error.  The unit being connected to or disconnected should only have 1 input or output. ";
                case RESULT.ERR_FILE_BAD: return "Error loading file. ";
                case RESULT.ERR_FILE_COULDNOTSEEK: return "Couldn't perform seek operation.  This is a limitation of the medium (ie netstreams) or the file format. ";
                case RESULT.ERR_FILE_EOF: return "End of file unexpectedly reached while trying to read essential data (truncated data?). ";
                case RESULT.ERR_FILE_NOTFOUND: return "File not found. ";
                case RESULT.ERR_FILE_UNWANTED: return "Unwanted file access occured.";
                case RESULT.ERR_FORMAT: return "Unsupported file or audio format. ";
                case RESULT.ERR_HTTP: return "A HTTP error occurred. This is a catch-all for HTTP errors not listed elsewhere. ";
                case RESULT.ERR_HTTP_ACCESS: return "The specified resource requires authentication or is forbidden. ";
                case RESULT.ERR_HTTP_PROXY_AUTH: return "Proxy authentication is required to access the specified resource. ";
                case RESULT.ERR_HTTP_SERVER_ERROR: return "A HTTP server error occurred. ";
                case RESULT.ERR_HTTP_TIMEOUT: return "The HTTP request timed out. ";
                case RESULT.ERR_INITIALIZATION: return "FMOD was not initialized correctly to support this function. ";
                case RESULT.ERR_INITIALIZED: return "Cannot call this command after System::init. ";
                case RESULT.ERR_INTERNAL: return "An error occured that wasnt supposed to.  Contact support. ";
                case RESULT.ERR_INVALID_HANDLE: return "An invalid object handle was used. ";
                case RESULT.ERR_INVALID_PARAM: return "An invalid parameter was passed to this function. ";
                case RESULT.ERR_INVALID_SPEAKER: return "An invalid speaker was passed to this function based on the current speaker mode. ";
                case RESULT.ERR_IRX: return "PS2 only.  fmodex.irx failed to initialize.  This is most likely because you forgot to load it. ";
                case RESULT.ERR_MEMORY: return "Not enough memory or resources. ";
                case RESULT.ERR_MEMORY_IOP: return "PS2 only.  Not enough memory or resources on PlayStation 2 IOP ram. ";
                case RESULT.ERR_MEMORY_SRAM: return "Not enough memory or resources on console sound ram. ";
                case RESULT.ERR_NEEDS2D: return "Tried to call a command on a 3d sound when the command was meant for 2d sound. ";
                case RESULT.ERR_NEEDS3D: return "Tried to call a command on a 2d sound when the command was meant for 3d sound. ";
                case RESULT.ERR_NEEDSHARDWARE: return "Tried to use a feature that requires hardware support.  (ie trying to play a VAG compressed sound in software on PS2). ";
                case RESULT.ERR_NEEDSSOFTWARE: return "Tried to use a feature that requires the software engine.  Software engine has either been turned off, or command was executed on a hardware channel which does not support this feature. ";
                case RESULT.ERR_NET_CONNECT: return "Couldn't connect to the specified host. ";
                case RESULT.ERR_NET_SOCKET_ERROR: return "A socket error occurred.  This is a catch-all for socket-related errors not listed elsewhere. ";
                case RESULT.ERR_NET_URL: return "The specified URL couldn't be resolved. ";
                case RESULT.ERR_NOTREADY: return "Operation could not be performed because specified sound is not ready. ";
                case RESULT.ERR_OUTPUT_ALLOCATED: return "Error initializing output device, but more specifically, the output device is already in use and cannot be reused. ";
                case RESULT.ERR_OUTPUT_CREATEBUFFER: return "Error creating hardware sound buffer. ";
                case RESULT.ERR_OUTPUT_DRIVERCALL: return "A call to a standard soundcard driver failed, which could possibly mean a bug in the driver or resources were missing or exhausted. ";
                case RESULT.ERR_OUTPUT_FORMAT: return "Soundcard does not support the minimum features needed for this soundsystem (16bit stereo output). ";
                case RESULT.ERR_OUTPUT_INIT: return "Error initializing output device. ";
                case RESULT.ERR_OUTPUT_NOHARDWARE: return "FMOD_HARDWARE was specified but the sound card does not have the resources nescessary to play it. ";
                case RESULT.ERR_OUTPUT_NOSOFTWARE: return "Attempted to create a software sound but no software channels were specified in System::init. ";
                case RESULT.ERR_PAN: return "Panning only works with mono or stereo sound sources. ";
                case RESULT.ERR_PLUGIN: return "An unspecified error has been returned from a 3rd party plugin. ";
                case RESULT.ERR_PLUGIN_MISSING: return "A requested output, dsp unit type or codec was not available. ";
                case RESULT.ERR_PLUGIN_RESOURCE: return "A resource that the plugin requires cannot be found. (ie the DLS file for MIDI playback) ";
                case RESULT.ERR_RECORD: return "An error occured trying to initialize the recording device. ";
                case RESULT.ERR_REVERB_INSTANCE: return "Specified Instance in FMOD_REVERB_PROPERTIES couldn't be set. Most likely because another application has locked the EAX4 FX slot. ";
                case RESULT.ERR_SUBSOUND_ALLOCATED: return "This subsound is already being used by another sound, you cannot have more than one parent to a sound.  Null out the other parent's entry first. ";
                case RESULT.ERR_TAGNOTFOUND: return "The specified tag could not be found or there are no tags. ";
                case RESULT.ERR_TOOMANYCHANNELS: return "The sound created exceeds the allowable input channel count.  This can be increased with System::setMaxInputChannels. ";
                case RESULT.ERR_UNIMPLEMENTED: return "Something in FMOD hasn't been implemented when it should be! contact support! ";
                case RESULT.ERR_UNINITIALIZED: return "This command failed because System::init or System::setDriver was not called. ";
                case RESULT.ERR_UNSUPPORTED: return "A command issued was not supported by this object.  Possibly a plugin without certain callbacks specified. ";
                case RESULT.ERR_UPDATE: return "On PS2, System::update was called twice in a row when System::updateFinished must be called first. ";
                case RESULT.ERR_VERSION: return "The version number of this file format is not supported. ";
                case RESULT.OK: return "No errors.";
                default: return "Unknown error.";
            }
        }
    }
}
