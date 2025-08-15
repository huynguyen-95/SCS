import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import environment from '../../env';

@Injectable({
  providedIn: 'root'
})
export class AlarmSystemService {
  private hubConnection: HubConnection | null = null;

  constructor() { }

  /**
   * Start SignalR connection to the alarm-system hub
   * @param groupId The premise ID to use as groupId query parameter
   */
  async startConnection(groupId: number): Promise<void> {
    try {
      // Build the connection with the alarm-system hub endpoint
      this.hubConnection = new HubConnectionBuilder()
        .withUrl(`${environment.apiUrl}/hubs/alarm-system?groupId=${groupId}`)
        .withAutomaticReconnect()
        .configureLogging(LogLevel.Information)
        .build();

      // Start the connection
      await this.hubConnection.start();
      console.log('SignalR connection started successfully for groupId:', groupId);

    } catch (error) {
      console.error('Error starting SignalR connection:', error);
      throw error;
    }
  }

  /**
   * Stop the SignalR connection
   */
  async stopConnection(): Promise<void> {
    if (this.hubConnection) {
      try {
        await this.hubConnection.stop();
        console.log('SignalR connection stopped');
      } catch (error) {
        console.error('Error stopping SignalR connection:', error);
      } finally {
        this.hubConnection = null;
      }
    }
  }

  /**
   * Check if the connection is established
   */
  isConnected(): boolean {
    return this.hubConnection?.state === 'Connected';
  }

  /**
   * Invoke a method on the hub
   * @param methodName The method name to invoke
   * @param args Arguments to pass to the method
   */
  async invokeMethod(methodName: string, ...args: any[]): Promise<any> {
    if (!this.hubConnection) {
      throw new Error('SignalR connection not established');
    }

    try {
      return await this.hubConnection.invoke(methodName, ...args);
    } catch (error) {
      console.error(`Error invoking method ${methodName}:`, error);
      throw error;
    }
  }

  /**
   * Register a callback for a specific hub method
   * @param methodName The method name to listen for
   * @param callback The callback function to execute
   */
  onMethod(methodName: string, callback: (...args: any[]) => void): void {
    if (!this.hubConnection) {
      throw new Error('SignalR connection not established');
    }

    this.hubConnection.on(methodName, callback);
  }

  /**
   * Unregister a callback for a specific hub method
   * @param methodName The method name to stop listening for
   */
  offMethod(methodName: string): void {
    if (this.hubConnection) {
      this.hubConnection.off(methodName);
    }
  }

  /**
   * Get the connection state
   */
  getConnectionState(): string {
    return this.hubConnection?.state || 'Disconnected';
  }
}
