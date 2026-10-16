export interface Order {
  id: number;
  customerName: string;
  total: number;
  status: string;
  items: OrderItem[];
}

export interface OrderItem {
  productId: number;
  productName: string;
  unitPrice: number;
  quantity: number;
}

export interface CreateOrderRequest {
  customerName: string;
  items?: OrderLineRequest[];
}

export interface OrderLineRequest {
  productId: number;
  productName: string;
  unitPrice: number;
  quantity: number;
}

export interface UpdateOrderStatusRequest {
  status: string;
}

export enum OrderStatus {
  Pending = 'Pending',
  Confirmed = 'Confirmed',
  Shipped = 'Shipped',
  Delivered = 'Delivered',
  Cancelled = 'Cancelled'
}