/**
 * Parser de BRCode (PIX Copia e Cola) no formato EMV QR Code.
 *
 * Formato TLV: cada campo = 2 chars (tag) + 2 chars (tamanho) + N chars (valor)
 * Tags compostas (26, 62) contêm sub-TLVs no mesmo formato.
 */

export interface BRCodeData {
  pixKey: string;
  amount: number;
  merchantName: string;
  city: string;
  txId: string;
  isValid: boolean;
}

/** Parse TLV (Tag-Length-Value) fields from an EMV string */
function parseTLV(data: string): Map<string, string> {
  const result = new Map<string, string>();
  let i = 0;
  while (i + 4 <= data.length) {
    const tag = data.substring(i, i + 2);
    const lenStr = data.substring(i + 2, i + 4);
    const len = parseInt(lenStr, 10);
    if (isNaN(len) || len < 0 || i + 4 + len > data.length) break;
    result.set(tag, data.substring(i + 4, i + 4 + len));
    i += 4 + len;
  }
  return result;
}

/** Parse a PIX BRCode (EMV payload) and extract payment data */
export function parseBRCode(payload: string): BRCodeData {
  const invalid: BRCodeData = {
    pixKey: '',
    amount: 0,
    merchantName: '',
    city: '',
    txId: '',
    isValid: false,
  };

  if (!payload || payload.trim().length < 20) return invalid;

  try {
    const clean = payload.trim();
    const tags = parseTLV(clean);

    // Tag 00: Payload Format Indicator (deve ser "01")
    if (tags.get('00') !== '01') return invalid;

    // Tag 26: Merchant Account Information (contém subtags)
    const tag26 = tags.get('26');
    if (!tag26) return invalid;
    const sub26 = parseTLV(tag26);

    // Subtag 00: GUI deve ser "BR.GOV.BCB.PIX"
    if (sub26.get('00') !== 'BR.GOV.BCB.PIX') return invalid;

    // Subtag 01: Chave PIX
    const pixKey = sub26.get('01') || '';

    // Tag 54: Valor da transação
    const amountStr = tags.get('54') || '0';
    const amount = parseFloat(amountStr) || 0;

    // Tag 59: Nome do recebedor
    const merchantName = tags.get('59') || '';

    // Tag 60: Cidade
    const city = tags.get('60') || '';

    // Tag 62: Dados adicionais (contém subtags)
    let txId = '';
    const tag62 = tags.get('62');
    if (tag62) {
      const sub62 = parseTLV(tag62);
      txId = sub62.get('05') || '';
    }

    return {
      pixKey,
      amount,
      merchantName,
      city,
      txId,
      isValid: pixKey.length > 0 && amount > 0,
    };
  } catch {
    return invalid;
  }
}
