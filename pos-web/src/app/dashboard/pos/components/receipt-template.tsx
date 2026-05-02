import { Sale } from "@/types/sales";
import { formatCurrency } from "@/lib/utils";

interface ReceiptTemplateProps {
    sale: Sale | null;
    companyName?: string;
}

export function ReceiptTemplate({ sale, companyName }: ReceiptTemplateProps) {
    if (!sale) return null;

    // Calculate Subtotal and Aggregate Taxes
    let subtotal = 0;
    const taxes: Record<string, { rate: number; sumAmount: number }> = {};

    sale.details.forEach(item => {
        // Find line total before tax (assuming unitPrice doesn't include tax yet, 
        // OR the lineTotal includes tax and we need to reverse engineer it. 
        // Based on SaleService, itemDto.UnitPrice is base price, and lineTotal = (Base * Qty) - Discount + VAT)

        let itemTaxSum = 0;

        if (item.vatDetails) {
            try {
                const parsedVats = JSON.parse(item.vatDetails);
                if (Array.isArray(parsedVats)) {
                    parsedVats.forEach((v: any) => {
                        itemTaxSum += v.Amount;
                        const taxKey = `${v.Name}-${v.Rate}`;
                        if (!taxes[taxKey]) {
                            taxes[taxKey] = { rate: v.Rate, sumAmount: 0 };
                        }
                        taxes[taxKey].sumAmount += v.Amount;
                    });
                }
            } catch (e) {
                console.error("Failed to parse VAT details for item", item.productName);
            }
        }

        const baseLineTotal = item.lineTotal - itemTaxSum;
        subtotal += baseLineTotal;
    });

    return (
        <div className="w-[80mm] p-2 font-mono text-sm leading-tight text-black bg-white mx-auto print:mx-0">
            <div className="text-center mb-4">
                {companyName && <h1 className="text-xl font-extrabold uppercase mb-1">{companyName}</h1>}
                <h2 className="text-lg font-bold uppercase">{sale.locationName}</h2>
                <div className="text-xs mt-1">
                    <p>Receipt: {sale.saleNumber}</p>
                    <p>{new Date(sale.saleDate).toLocaleString()}</p>
                </div>
            </div>

            <div className="border-b border-black border-dashed my-2" />

            <div className="flex flex-col gap-1">
                {sale.details.map((item, index) => (
                    <div key={index} className="flex flex-col">
                        <span className="font-semibold">{item.productName}</span>
                        <div className="flex justify-between text-xs">
                            <span>{item.quantity} x {formatCurrency(item.unitPrice)}</span>
                            <span>{formatCurrency(item.lineTotal)}</span>
                        </div>
                    </div>
                ))}
            </div>

            <div className="border-b border-black border-dashed my-2" />

            <div className="space-y-1 text-right">
                <div className="flex justify-between text-sm">
                    <span>Subtotal</span>
                    <span>{formatCurrency(subtotal)}</span>
                </div>

                {Object.entries(taxes).map(([key, data]) => {
                    const [name] = key.split('-');
                    return (
                        <div key={key} className="flex justify-between text-xs text-muted-foreground">
                            <span>{name} ({data.rate}%)</span>
                            <span>{formatCurrency(data.sumAmount)}</span>
                        </div>
                    );
                })}

                <div className="border-b border-black border-dashed my-1" />

                <div className="flex justify-between font-bold text-base">
                    <span>TOTAL</span>
                    <span>{formatCurrency(sale.totalAmount)}</span>
                </div>
                <div className="flex justify-between text-xs mt-1">
                    <span>Method:</span>
                    <span>{sale.paymentMethod}</span>
                </div>
            </div>

            <div className="border-b border-black border-dashed my-4" />

            <div className="text-center text-xs space-y-1">
                <p>Thank you for your business!</p>
            </div>
        </div>
    );
}
