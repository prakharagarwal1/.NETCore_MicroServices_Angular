export interface Cart {
  cartId: number;
  items: CartItem[];
  total: number;
  itemCount: number;
}

export interface CartItem {
  productId: number;
  productName: string;
  unitPrice: number;
  quantity: number;
  category: string;
  lineTotal: number;
}

export interface AddToCartRequest {
  productId: number;
  productName: string;
  unitPrice: number;
  quantity: number;
  category?: string;
}

export interface UpdateCartItemRequest {
  quantity: number;
}