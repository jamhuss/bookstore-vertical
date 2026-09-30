import { useState } from 'react';
import { getApiErrorMessage, ordersApi } from '../api';
import type { Book, Order, OrderInput, OrderItemInput } from '../types';
import { Modal } from './Modal';

interface OrderModalProps {
    order: Order | null;
    books: Book[];
    onClose: () => void;
    onSaved: () => void;
}

export function OrderModal({ order, books, onClose, onSaved }: OrderModalProps) {
    const [email, setEmail] = useState(() => order?.userEmail ?? '');
    const [items, setItems] = useState<OrderItemInput[]>(() =>
        order
            ? order.items.map((i) => ({ bookId: i.bookId, quantity: i.quantity }))
            : books.length
              ? [{ bookId: books[0].id, quantity: 1 }]
              : [],
    );
    const [error, setError] = useState<string | null>(null);
    const [saving, setSaving] = useState(false);

    function priceOf(bookId: number) {
        return books.find((b) => b.id === bookId)?.price ?? 0;
    }

    const estimatedTotal = items.reduce((sum, i) => sum + priceOf(i.bookId) * i.quantity, 0);

    function addRow() {
        if (!books.length) return;
        setItems([...items, { bookId: books[0].id, quantity: 1 }]);
    }

    function updateRow(index: number, patch: Partial<OrderItemInput>) {
        setItems(items.map((it, i) => (i === index ? { ...it, ...patch } : it)));
    }

    function removeRow(index: number) {
        setItems(items.filter((_, i) => i !== index));
    }

    async function save(e: React.FormEvent) {
        e.preventDefault();
        setSaving(true);
        setError(null);
        const input: OrderInput = { userEmail: email, items };
        try {
            if (order) {
                await ordersApi.update(order.id, input);
            } else {
                await ordersApi.create(input);
            }
            onSaved();
        } catch (err) {
            setError(getApiErrorMessage(err));
        } finally {
            setSaving(false);
        }
    }

    return (
        <Modal title={order ? `Edit order #${order.id}` : 'New order'} onClose={onClose}>
            <form onSubmit={save} className="form">
                <label>
                    Customer email
                    <input
                        type="email"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                    />
                </label>

                <div className="line-editor">
                    <div className="line-editor-head">
                        <span>Books</span>
                        <button type="button" onClick={addRow}>
                            + Add line
                        </button>
                    </div>

                    {items.map((it, index) => (
                        <div className="line-row" key={index}>
                            <select
                                value={it.bookId}
                                onChange={(e) =>
                                    updateRow(index, { bookId: Number(e.target.value) })
                                }
                            >
                                {books.map((b) => (
                                    <option key={b.id} value={b.id}>
                                        {b.title} ({b.price.toFixed(2)} kr)
                                    </option>
                                ))}
                            </select>
                            <input
                                type="number"
                                min="1"
                                value={it.quantity}
                                onChange={(e) =>
                                    updateRow(index, { quantity: Number(e.target.value) })
                                }
                            />
                            <span className="line-total">
                                {(priceOf(it.bookId) * it.quantity).toFixed(2)} kr
                            </span>
                            <button
                                type="button"
                                className="icon-button"
                                aria-label="Remove line"
                                onClick={() => removeRow(index)}
                            >
                                &times;
                            </button>
                        </div>
                    ))}

                    {items.length === 0 && <p className="muted">Add at least one book.</p>}
                </div>

                <div className="total-row">
                    <span>Estimated total</span>
                    <strong>{estimatedTotal.toFixed(2)} kr</strong>
                </div>

                {error && <p className="error">{error}</p>}

                <div className="form-actions">
                    <button type="button" onClick={onClose} disabled={saving}>
                        Cancel
                    </button>
                    <button type="submit" className="primary" disabled={saving}>
                        {saving ? 'Saving…' : 'Save'}
                    </button>
                </div>
            </form>
        </Modal>
    );
}
