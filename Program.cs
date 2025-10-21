using NAudio.Wave;
using WebRtcVadSharp;

namespace VoiceTrigger
{
    class Program
    {
        // 볼륨 계산 함수 (RMS 방식)
        static float CalculateVolume(byte[] audioData)
        {
            float sum = 0;
            for (int i = 0; i < audioData.Length - 1; i += 2)
            {
                // 16-bit PCM 샘플 읽기
                short sample = (short)((audioData[i + 1] << 8) | audioData[i]);
                float normalizedSample = sample / 32768f; // -1.0 ~ 1.0으로 정규화
                sum += normalizedSample * normalizedSample;
            }
            return (float)Math.Sqrt(sum / (audioData.Length / 2));
        }

        static void Main(string[] args)
        {
            Console.WriteLine("음성 감지 시작 (종료: 아무 키나 누르기)");

            // 현재 연결된 마이크 정보 출력
            if (WaveInEvent.DeviceCount > 1)
            {
                var capabilities = WaveInEvent.GetCapabilities(1);
                Console.WriteLine($"마이크: {capabilities.ProductName}\n");
            }
            else
            {
                Console.WriteLine("마이크: Device 1이 없습니다\n");
            }

            // WebRTC VAD 초기화
            var vad = new WebRtcVad
            {
                OperatingMode = OperatingMode.LowBitrate, // 낮은 민감도
                SampleRate = SampleRate.Is16kHz,
                FrameLength = FrameLength.Is20ms
            };

            // 마이크 입력 설정 (16kHz, 16-bit, Mono) - Device 1 사용
            var waveIn = new WaveInEvent
            {
                DeviceNumber = 1, // Device 1 사용
                WaveFormat = new WaveFormat(16000, 16, 1), // 16kHz, 16-bit, Mono
                BufferMilliseconds = 20 // 20ms 프레임
            };

            // 연속 감지 카운터 (최소 5 프레임 연속 감지 필요)
            int consecutiveSpeechFrames = 0;
            const int minConsecutiveFrames = 5;
            bool isCurrentlySpeaking = false;

            // 마이크 데이터 수신 이벤트
            waveIn.DataAvailable += (sender, e) =>
            {
                try
                {
                    // 20ms 프레임: 16000 Hz * 0.02s = 320 samples * 2 bytes = 640 bytes
                    if (e.BytesRecorded >= 640)
                    {
                        // 640바이트 단위로 처리
                        byte[] frame = new byte[640];
                        Array.Copy(e.Buffer, 0, frame, 0, 640);

                        // 볼륨 체크 (너무 작은 소리는 무시)
                        float volume = CalculateVolume(frame);
                        const float volumeThreshold = 0.03f; // 볼륨 임계값 (조정 가능)

                        if (volume < volumeThreshold)
                        {
                            consecutiveSpeechFrames = 0;
                            isCurrentlySpeaking = false;
                            return;
                        }

                        // VAD로 음성 감지
                        bool isSpeech = vad.HasSpeech(frame);

                        if (isSpeech)
                        {
                            consecutiveSpeechFrames++;

                            // 연속으로 일정 프레임 이상 감지되면 음성으로 판단
                            if (consecutiveSpeechFrames >= minConsecutiveFrames && !isCurrentlySpeaking)
                            {
                                isCurrentlySpeaking = true;
                                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] 감지");
                            }
                        }
                        else
                        {
                            // 음성이 아니면 카운터 리셋
                            if (consecutiveSpeechFrames > 0)
                            {
                                consecutiveSpeechFrames = 0;
                                isCurrentlySpeaking = false;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"오류: {ex.Message}");
                }
            };

            // 음성 감지 시작
            try
            {
                waveIn.StartRecording();
                Console.ReadKey(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"마이크 초기화 오류: {ex.Message}");
                Console.WriteLine("\n가능한 해결 방법:");
                Console.WriteLine("1. 마이크가 연결되어 있는지 확인하세요.");
                Console.WriteLine("2. 마이크 권한이 허용되어 있는지 확인하세요.");
                Console.WriteLine("3. 다른 프로그램이 마이크를 사용하고 있지 않은지 확인하세요.");
            }
            finally
            {
                waveIn.StopRecording();
                waveIn.Dispose();
                vad.Dispose();
            }
        }
    }
}
