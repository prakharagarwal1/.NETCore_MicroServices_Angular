export interface Order {
  id: number;
  customerName: string;
  total: number;
  status: string;
}

export interface CreateOrderRequest {
  customerName: string;
  total: number;
}