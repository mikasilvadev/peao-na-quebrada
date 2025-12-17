# Peao Na Quebrada

Peao Na Quebrada e um projeto solo feito no Unity 6 que busca entregar um mundo aberto leve e otimizado.

## Visao Rapida
- Motor principal pensado para PCs modestos (4 GB RAM, grafico integrado) com visual low poly.
- Roadmap incremental: mapa crescerá bairro a bairro.

## Estrutura de Codigo
- Assets/Project/Scripts/Player
  - PlayerMovement.cs: Módulo central de locomoção e máquina de estados.
  - PlayerInputManager.cs e PlayerInput.cs: Abstração dos eventos do Input System.
  - PlayerCameraManager.cs: Controle das câmeras Cinemachine e exposição do modo FPC.
- Assets/Project/Scripts/Motorcycle
  - MotoInteraction.cs: Gerenciamento da interação jogador/moto e sincronização de scripts.
  - MotoPhysics.cs: Estabilidade, detecçao de tombamento e ajustes de suspensão.
  - MotorcycleController.cs: Aplica torque, ângulo de direção e frenagem nas WheelColliders.
- Input System
  - Assets/Project/Inputs/InputActions/PlayerControls.inputactions: Mapas Player e Vehicle.

## Pilares de Design
1. Veículos: Customização modular, física arcade realistica e danos por peça.
2. Mundo Vivo: Trânsito imprevisível e NPCs com comportamentos variados.
3. Sobrevivencia: Riscos de assaltos e pressão policial.
4. Acessivel a Hardware Fraco: Modelos low poly, pooling agressivo, culling de fisica e atlas de texturas.
