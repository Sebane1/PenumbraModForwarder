# **Application Design Document**

## **Overview**

This project aims to refactor a monolithic application into a modular, cross-platform system. The new application consists of three components:

1. **UI** - Interactive elements for users, built with Avalonia using the MVVM framework.
2. **Background Service** - Handles computationally expensive tasks, communicates with the UI via WebSockets.
3. **Watchdog** - Monitors and manages the UI and Background Service, ensuring fault tolerance and user transparency.

The system will prioritize modularity, ease of testing, and maintainability. It will be cross-platform, targeting both Windows and Linux environments.

---

## **System Architecture**

### **UI**
- **Framework**: Avalonia with MVVM.
- **Responsibilities**:
    - Provide user interaction components (e.g., textboxes, file selectors).
    - Display real-time progress and statuses received from the Background Service.
    - Handle system tray integration for minimization.
    - Notify users of errors or crashes in the Background Service.
- **Communication**:
    - Uses WebSockets to send/receive data from the Background Service.
    - System tray integration will rely on native platform APIs via Avalonia extensions or libraries.

### **Background Service**
- **Responsibilities**:
    - Execute expensive tasks such as file processing or conversions.
    - Maintain a task queue with state persistence for restart/resume functionality.
    - Report progress, plans, and errors to the UI via WebSockets.
- **Communication**:
    - Acts as a WebSocket server, sending updates and receiving commands from the UI.
- **Features**:
    - Process queue with task state persistence.
    - Support for multiple types of tasks, each with dedicated progress reports.

### **Watchdog**
- **Responsibilities**:
    - Start and monitor the UI and Background Service.
    - Ensure both processes are running and restart them if they crash.
    - Notify the UI of Background Service crashes.
- **Communication**:
    - Relays crash events to the UI.
- **Design**:
    - Hidden console window, running as a background process.
    - Cross-platform support for process management.

---

## **Key Features**

### **Cross-Platform Compatibility**
- Use .NET 9 for cross-platform support.
- Platform-specific adjustments for:
    - System tray integration.
    - Hidden console window behavior in the Watchdog.

### **Modularity**
- Design each component (UI, Background Service, Watchdog) to work independently.
- Define clear interfaces and communication protocols.

### **Fault Tolerance**
- Persistent task queue to recover from unexpected shutdowns.
- Watchdog ensures all components remain operational.

### **Ease of Testing**
- Each component can be tested independently:
    - UI with Avalonia test tools.
    - Background Service with mock WebSocket clients.
    - Watchdog with process management simulation tools.

---

## **Implementation Plan**

### **Phase 1: Core Setup**
1. Create repositories for each component.
2. Setup common libraries for shared models and communication protocols (e.g., WebSocket message schemas).

### **Phase 2: Background Service**
1. Implement a task queue with state persistence.
2. Build WebSocket server for progress and command handling.
3. Integrate basic task execution and progress reporting.

### **Phase 3: UI**
1. Design basic MVVM structure with Avalonia.
2. Create a WebSocket client to communicate with the Background Service.
3. Implement system tray functionality.

### **Phase 4: Watchdog**
1. Implement process management for UI and Background Service.
2. Add monitoring to detect crashes and restart processes.
3. Notify UI on Background Service failures.

### **Phase 5: Integration and Testing**
1. Integrate UI and Background Service communication.
2. Test Watchdog with various failure scenarios.
3. Optimize for cross-platform compatibility.

---

## **Technical Details**

### **Technologies**
- **UI**: Avalonia, ReactiveUI.
- **Background Service**: .NET 7+, WebSocket library (e.g., System.Net.WebSockets).
- **Watchdog**: Process management using `Process` in .NET.

### **Data Persistence**
- Use a lightweight database or JSON for saving task queue states (e.g., LiteDB or System.Text.Json).

### **Message Protocol**
Define a standard WebSocket message format:
```json
{
  "type": "progress_update",
  "task_id": "123",
  "status": "in_progress",
  "progress": 75,
  "message": "Extracting files..."
}
```
---

## **Next Steps**

- **Confirm Design**: Review the architecture and adjust based on additional requirements or feedback.
- **Define Task Types**: Enumerate specific tasks the Background Service will handle (e.g., file extraction, conversion).
- **Build Skeleton Code**: Initialize repositories with basic project structure for all three components.