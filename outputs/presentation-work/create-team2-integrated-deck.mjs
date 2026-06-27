import fs from "node:fs/promises";
import path from "node:path";
import { Presentation, PresentationFile } from "@oai/artifact-tool";

const project = "/Users/taeyunkim/Library/Mobile Documents/com~apple~CloudDocs/development/Team2-Project";
const tmp = "/var/folders/xt/b_w1p4653pgg00287tclh7kw0000gn/T/codex-presentations/manual-team2-breakout/team2-rpg-breakout/tmp_mid_integrated";
const previewDir = path.join(tmp, "preview");
const layoutDir = path.join(tmp, "layout");
const outPptx = path.join(project, "outputs", "team2-rpg-breakout-integrated.pptx");

const keySlides = "/var/folders/xt/b_w1p4653pgg00287tclh7kw0000gn/T/codex-presentations/manual-team2-breakout/team2-rpg-breakout/tmp_mid/key_export/template-inspect/source-slides";
const A = {
  cover: path.join(keySlides, "source-slide-01.png"),
  userPage2: path.join(keySlides, "source-slide-02.png"),
  bg: path.join(project, "Assets/Resources/TitleSceneArtwork/background.png"),
  titleImage: path.join(project, "Assets/Art/UI/제목 이미지/손오공 제목.png"),
  ball: path.join(project, "Assets/Art/Ball/Idle_up/Idle_up 1.png"),
  nimbus: path.join(project, "Assets/Resources/Paddle/NimbusCloud/nimbus_idle_big.png"),
  enemy: path.join(project, "Assets/Art/Enemy/idle/idle1.png"),
  cow: path.join(project, "Assets/Art/CowKing/Move/00.PNG"),
  kirby: "/tmp/team2_ppt_assets/kirby-pink.png",
  concept: "/tmp/team2_ppt_assets/concept/컨셉아트/KakaoTalk_Photo_2026-05-01-21-30-40.png",
  level: "/Users/taeyunkim/Library/Mobile Documents/com~apple~CloudDocs/Downloads/IMG_2E4EE4A97CEB-1.jpeg",
};

const W = 1280;
const H = 720;
const C = {
  text: "#fff8eb",
  sub: "#ffd891",
  accent: "#ff9b2a",
  panel: "rgba(255, 245, 221, 0.94)",
  dark: "#2a1b18",
  muted: "#7d5143",
};

async function read(file) {
  return await fs.readFile(file);
}

function contentType(file) {
  return [".jpg", ".jpeg"].includes(path.extname(file).toLowerCase()) ? "image/jpeg" : "image/png";
}

function text(slide, value, left, top, width, height, options = {}) {
  const shape = slide.shapes.add({
    geometry: "textbox",
    position: { left, top, width, height },
    fill: "none",
    line: { style: "solid", fill: "none", width: 0 },
  });
  shape.text = value;
  shape.text.style = {
    fontFace: "Apple SD Gothic Neo",
    fontSize: options.size ?? 25,
    bold: options.bold ?? false,
    color: options.color ?? C.text,
    alignment: options.align ?? "left",
  };
  return shape;
}

function shape(slide, geometry, left, top, width, height, fill, line = "none", radius = 0) {
  return slide.shapes.add({
    geometry,
    position: { left, top, width, height },
    fill,
    line: { style: "solid", fill: line, width: line === "none" ? 0 : 2 },
    ...(radius ? { borderRadius: radius } : {}),
  });
}

async function image(slide, file, left, top, width, height, fit = "contain", alt = "") {
  return slide.images.add({
    blob: await read(file),
    contentType: contentType(file),
    alt,
    fit,
    position: { left, top, width, height },
  });
}

async function darkBg(slide) {
  slide.background.fill = "#211316";
  await image(slide, A.bg, 0, 0, W, H, "cover", "background");
  shape(slide, "rect", 0, 0, W, H, "rgba(18, 10, 10, 0.76)");
}

function midHeader(slide, number, title) {
  text(slide, number, 1158, 34, 40, 24, { size: 13, color: "#d8b98b", align: "right" });
  shape(slide, "rect", 68, 148, 104, 6, C.accent);
  text(slide, title, 430, 84, 420, 64, { size: 40, bold: true, color: C.text, align: "center" });
}

function bulletBody(slide, lines, left = 208, top = 214, width = 510, size = 22) {
  text(slide, lines.join("\n"), left, top, width, 380, { size, color: C.text });
}

async function visualPanel(slide, left, top, width, height, title = "") {
  shape(slide, "roundRect", left, top, width, height, "rgba(255, 245, 221, 0.12)", "rgba(255, 216, 145, 0.42)", 26);
  if (title) text(slide, title, left + 26, top + 22, width - 52, 30, { size: 18, bold: true, color: C.sub });
}

async function fullImageSlide(deck, file) {
  const slide = deck.slides.add();
  slide.background.fill = "#211316";
  await image(slide, file, 0, 0, W, H, "cover", "source slide");
}

async function main() {
  await fs.mkdir(path.dirname(outPptx), { recursive: true });
  await fs.rm(previewDir, { recursive: true, force: true });
  await fs.rm(layoutDir, { recursive: true, force: true });
  await fs.mkdir(previewDir, { recursive: true });
  await fs.mkdir(layoutDir, { recursive: true });

  const deck = Presentation.create({ slideSize: { width: W, height: H } });

  await fullImageSlide(deck, A.cover);
  await fullImageSlide(deck, A.userPage2);

  let slide = deck.slides.add();
  await darkBg(slide);
  midHeader(slide, "01", "주제");
  bulletBody(slide, [
    "• 벽돌깨기",
    "• 벽돌 대신 몬스터를 잡으면 어떻냐는 아이디어",
    "  ◦ → RPG 요소로 발전시킴",
    "  ◦ 경험치, 스탯 강화 등의 성장요소",
  ], 248, 214, 620, 25);
  await image(slide, A.enemy, 850, 260, 130, 130, "contain", "monster");
  await image(slide, A.ball, 760, 390, 110, 110, "contain", "ball");
  await image(slide, A.titleImage, 930, 180, 150, 300, "contain", "title mark");

  slide = deck.slides.add();
  await darkBg(slide);
  midHeader(slide, "02", "기획 구체화");
  bulletBody(slide, [
    "• 지형지물과 그 위의 몬스터가 스크롤 되어",
    "  화면 밑으로 내려옴.",
    "• 공은 지형지물에 반사되며, 몬스터 접촉 시 공격함.",
    "• 처치되지 못한 몬스터는 패들에 접촉 시",
    "  유저의 체력을 깎음.",
  ], 135, 215, 520, 21);
  await visualPanel(slide, 684, 86, 420, 556, "구조");
  await image(slide, A.ball, 760, 190, 82, 82, "contain", "ball");
  await image(slide, A.enemy, 910, 220, 96, 96, "contain", "enemy");
  await image(slide, A.nimbus, 780, 496, 260, 80, "contain", "paddle");
  shape(slide, "line", 820, 262, 180, -5, "none", "#ff5fa3");
  shape(slide, "line", 1000, 257, -145, 235, "none", "#ff5fa3");

  slide = deck.slides.add();
  await darkBg(slide);
  midHeader(slide, "03", "컨셉");
  bulletBody(slide, [
    "• 지형지물, 몬스터의 스크롤 다운 → 유저가 상승한다.",
    "• 손오공이 근두운을 타고 상승한다로 발전시킴",
    "• 손오공: 공",
    "  ◦ 커비, 샐리의 법칙처럼 동그랗게 디자인",
    "• 근두운: 패들",
    "• 화염산 에피소드 차용",
    "  ◦ 손오공이 화염산을 올라 우마왕을 처치",
    "  ◦ 산을 “오른다”는 점과 게임 구조가 일치",
    "  ◦ 보스가 명확",
  ], 100, 190, 690, 19);
  await image(slide, A.kirby, 820, 185, 160, 160, "contain", "kirby");
  await image(slide, A.ball, 990, 200, 110, 110, "contain", "goku ball");
  await image(slide, A.cow, 850, 405, 220, 220, "contain", "cow king");

  slide = deck.slides.add();
  await darkBg(slide);
  midHeader(slide, "04", "RPG");
  bulletBody(slide, [
    "• 정석적인 RPG 구조",
    "  ◦ 쫄을 잡으며 성장 → 보스와의 배틀 → Clear",
    "• 단일 스테이지 구성",
    "  ◦ 리소스 절약",
    "  ◦ 유저의 학습에 용이",
    "  ◦ 세이브 포인트를 없애 밸런스를 잡음",
    "• 로그라이크처럼 폭발적인 성장 제공",
    "• 인플레이션을 염두에 둔 밸런싱으로",
    "  유저의 성장 유인 제공, 성장요소 강조",
  ], 112, 190, 650, 20);
  await visualPanel(slide, 805, 172, 315, 360, "성장 루프");
  text(slide, "쫄 처치\n↓\n성장\n↓\n보스전\n↓\nClear", 882, 230, 160, 250, { size: 29, bold: true, color: C.sub, align: "center" });

  slide = deck.slides.add();
  await darkBg(slide);
  midHeader(slide, "05", "레퍼런스");
  bulletBody(slide, [
    "• 알카노이드",
    "  ◦ 아이템, 스킬 등 변주 요소가 있는 고전 벽돌깨기",
    "  ◦ 공이 공중에 떠 있는 동안 조작 요소가 부족함",
    "  ◦ 스킬을 통해 유저 조작 개입 요소를 늘림",
    "• 스와이프 벽돌깨기",
    "  ◦ 속도감과 쾌감",
    "  ◦ 폭발적인 성장을 달성했을 때",
    "    벽돌깨기가 줄 수 있는 경험",
  ], 112, 190, 690, 20);
  await visualPanel(slide, 830, 175, 300, 360, "핵심 경험");
  text(slide, "속도감\n+\n스킬 개입\n+\n폭발 성장", 880, 260, 200, 150, { size: 30, bold: true, color: C.sub, align: "center" });

  slide = deck.slides.add();
  await darkBg(slide);
  midHeader(slide, "06", "기획 통합");
  bulletBody(slide, [
    "• 벽돌깨기의 기본 규칙은 유지",
    "• 화면 의미는 RPG 전투로 전환",
    "• 손오공 / 근두운 / 화염산 / 우마왕으로 컨셉 고정",
    "• 성장 곡선은 레벨디자인 그래프로 관리",
  ], 120, 205, 590, 24);
  await image(slide, A.level, 750, 180, 390, 315, "contain", "combat graph");
  await image(slide, A.concept, 850, 510, 210, 145, "cover", "concept");

  for (const [index, item] of deck.slides.items.entries()) {
    const stem = `slide-${String(index + 1).padStart(2, "0")}`;
    await fs.writeFile(path.join(previewDir, `${stem}.png`), new Uint8Array(await (await deck.export({ slide: item, format: "png", scale: 1 })).arrayBuffer()));
    await fs.writeFile(path.join(layoutDir, `${stem}.layout.json`), await (await item.export({ format: "layout" })).text());
  }

  const pptx = await PresentationFile.exportPptx(deck);
  await pptx.save(outPptx);
  console.log(outPptx);
}

main().catch((error) => {
  console.error(error);
  process.exitCode = 1;
});
