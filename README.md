# VoiceTrigger - WebRTC VAD 음성 감지 시스템

WebRTC VAD(Voice Activity Detection)를 사용하여 실시간으로 마이크 입력에서 사람의 음성을 감지하는 C# 콘솔 애플리케이션입니다.

## 기능

- 실시간 마이크 입력 감지 (AirPods 등 모든 마이크 장치 지원)
- WebRTC VAD를 사용한 정확한 음성 활동 감지
- 음성 감지 시 콘솔에 타임스탬프와 함께 "감지" 메시지 출력
- 연속된 음성에 대해 여러 번 감지 가능

## 기술 스택

- **.NET 9.0** - C# 콘솔 애플리케이션
- **NAudio 2.2.1** - 오디오 입력 캡처
- **WebRtcVadSharp 1.3.2** - WebRTC 음성 활동 감지

## 사용 방법

### 빌드

```bash
cd VoiceTrigger
dotnet build
```

### 실행

```bash
dotnet run
```

### 종료

프로그램 실행 중 아무 키나 누르면 종료됩니다.

## 설정

프로그램은 다음과 같은 오디오 설정을 사용합니다:

- **샘플레이트**: 16kHz
- **비트 깊이**: 16-bit
- **채널**: Mono (단일 채널)
- **프레임 길이**: 20ms
- **VAD 민감도**: VeryAggressive (가장 민감한 모드)

필요에 따라 `Program.cs`에서 설정을 변경할 수 있습니다:

```csharp
var vad = new WebRtcVad
{
    OperatingMode = OperatingMode.VeryAggressive, // Quality, LowBitrate, Aggressive, VeryAggressive
    SampleRate = SampleRate.Is16kHz,              // Is8kHz, Is16kHz, Is32kHz, Is48kHz
    FrameLength = FrameLength.Is20ms              // Is10ms, Is20ms, Is30ms
};
```

## 동작 원리

1. NAudio의 `WaveInEvent`를 사용하여 마이크에서 16kHz, 16-bit, Mono 오디오 스트림 캡처
2. 20ms 단위(640바이트)로 오디오 데이터를 처리
3. WebRTC VAD를 사용하여 각 프레임에서 음성 활동 감지
4. 음성이 감지되면 타임스탬프와 함께 "감지" 메시지 출력

## 문제 해결

### 마이크가 감지되지 않는 경우

1. 마이크가 올바르게 연결되어 있는지 확인
2. Windows 설정 > 개인정보 > 마이크에서 앱의 마이크 액세스 권한 확인
3. 다른 프로그램이 마이크를 사용하고 있지 않은지 확인

### 감지가 너무 민감하거나 둔감한 경우

`OperatingMode`를 조정:
- `Quality`: 가장 덜 민감 (품질 우선)
- `LowBitrate`: 낮은 비트레이트용
- `Aggressive`: 공격적
- `VeryAggressive`: 매우 공격적 (가장 민감)

## 라이선스

이 프로젝트는 교육 및 개발 목적으로 자유롭게 사용할 수 있습니다.

## 참고 자료

- [WebRtcVadSharp GitHub](https://github.com/ladenedge/WebRtcVadSharp)
- [NAudio GitHub](https://github.com/naudio/NAudio)
