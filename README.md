# AR Educational Mini-Game (Unity, C#)

---

## Overview
This Unity-based AR educational mini-game is designed to help children recognize numbers through interactive gameplay. It features real-time AR object placement, multi-language (English/Japanese) dialogue, voice feedback, and typewriter-style animated text, providing a playful and immersive learning experience.

---

## Key Features
- **AR Number Recognition:** Players find and select target numbers in an AR environment.
- **Multilingual Dialogue & Voice:** Supports both English and Japanese, including spoken voice and text.
- **Typewriter Dialogue UI:** Text appears with animated typewriter effect, synced with audio.
- **Audio & Visual Feedback:** Instant feedback for correct and incorrect answers (visual effects + sound).
- **Modular & Extensible:** UI, logic, and dialogue system designed for easy extension or reuse.

---

## Main Scripts
### **Manager.cs**
- Handles game state, AR object spawning, touch controls, scoring, and scene transitions.
- Manages language switching, audio, and connects with the dialogue system.

### **DialogueUIController.cs**
- Controls the full dialogue system: text display (with typewriter animation), voice playback, feedback, and multi-language support.
- Dynamically switches between dialogue branches (main/feedback/win) based on game events.

---

## How It Works
1. **Game Start:** The player is prompted to find a random number (with voice + animated text).
2. **AR Spawning:** Numbers are spawned randomly on detected AR planes.
3. **Player Interaction:** Tap to select; the system checks correctness and gives feedback.
4. **Multi-language:** All dialogue, text, and audio can switch between English and Japanese at runtime.

---

## Demo & Blog
- **Demo Video & Technical Blog:**  
https://kenji-dev.vercel.app/blog/AR-Educational-Mobile-Game-Log

---

## Usage
- **Unity Version:** 2021.3 LTS or newer (AR Foundation required)
- **How to Run:**  
  1. Open the project in Unity.  
  2. Set up AR Foundation/ARCore/ARKit for your target device.  
  3. Assign audio and dialogue assets in the Inspector as needed.  
  4. Build to your mobile device.

---

## Code Highlights
- Clean, well-commented C# scripts with clear modular separation.
- Public fields include Inspector [Tooltip]s for rapid understanding.
- Typewriter and audio logic fully decoupled for future extensibility.

---
